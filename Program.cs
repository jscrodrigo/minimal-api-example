using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TasksDB"));
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("GetPhrase", async () =>
    await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes"));

app.MapGet("/tasks", async (AppDbContext dbContext) =>
    {
        var tasks = await dbContext.Tasks.ToListAsync();

        if (!tasks.Any())
        {
            throw new Exception("Tasks not found!");
        }

        return tasks;
    });

app.MapPost("/tasks/create", async (Task task, AppDbContext dbContext) =>
    {
        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();
        return Results.Created($"/tasks/create/{task.Id}", task);
    });

app.Run();

class Task
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public bool IsDone { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Task> Tasks => Set<Task>();
}