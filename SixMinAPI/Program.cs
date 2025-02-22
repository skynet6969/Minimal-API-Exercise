using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SixMinAPI.Data;
using SixMinAPI.Dtos;
using SixMinAPI.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Connector for Database
var sqlConBuilder = new SqlConnectionStringBuilder();
sqlConBuilder.ConnectionString = builder.Configuration.GetConnectionString("SqlDbConnection");
sqlConBuilder.UserID = builder.Configuration["UserId"];
sqlConBuilder.Password = builder.Configuration["Password"];

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(sqlConBuilder.ConnectionString));
builder.Services.AddScoped<ICommandRepo, CommandRepo>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//get all commands
app.MapGet("api/v1/commands", async(ICommandRepo repo, IMapper mapper) => {
    var commands = await repo.GetAllCommands();
    return Results.Ok(mapper.Map<IEnumerable<CommandReadDto>>(commands));
});

//get a single value
app.MapGet("api/v1/commands/{id}", async(ICommandRepo repo, IMapper mapper, int id) => {
    var command = await repo.GetCommandById(id);
    if (command != null){
        return Results.Ok(mapper.Map<CommandReadDto>(command));
    }
    return Results.NotFound();
});

//add a value
app.MapPost("api/v1/commands", async(ICommandRepo repo, IMapper mapper, CommandCreateDto cmdCreateDto) => {

    var commandModel = mapper.Map<Command>(cmdCreateDto);
    
    await repo.CreateCommand(commandModel);
    await repo.SaveChanges();
    var cmdReadDto = mapper.Map<CommandReadDto>(commandModel);
    return Results.Created($"api/v1/commands/{cmdReadDto.ID}", cmdCreateDto);
});

//update a value
app.MapPut("api/v1/commands/{id}", async(ICommandRepo repo, IMapper mapper, int id, CommandUpdateDto cmdUpdateDto) => {
    var command = await repo.GetCommandById(id);
    if (command == null){
        return Results.NotFound();
    }
    mapper.Map(cmdUpdateDto, command);
    await repo.SaveChanges(); 
    return Results.NoContent();

});

app.MapDelete("api/v1/commands/{id}", async(ICommandRepo repo, IMapper mapper, int id) => {
    var command = await repo.GetCommandById(id);
    if (command == null){
        return Results.NotFound();
    }
    repo.DeleteCommand(command);
    await repo.SaveChanges();
    return Results.NoContent();
});

app.Run();
