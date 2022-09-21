using Microsoft.EntityFrameworkCore;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<TodoDB>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapPost("/todoitems", async (TodoDTO tododto, TodoDB db) =>
 {
 var Todoitem = new Todo
 {
     Name = tododto.Name,
     IsCompleted = tododto.IsCompleted

 };
     db.Todos.Add(Todoitem);
     await db.SaveChangesAsync();

     return Results.Created($"/todoitems/{Todoitem.Id}", new TodoDTO(Todoitem));

 });

app.MapGet("/todoitems", async (TodoDB db) =>
 await  db.Todos.Select(x => new TodoDTO(x)).ToListAsync());

app.MapGet("/todoitems/completed", async (TodoDB db) =>
await db.Todos.Where(t => t.IsCompleted).Select(x => new TodoDTO(x)).ToListAsync());

app.MapGet("/todoitems/{id}", async (TodoDB db,int id)=>
    await db.Todos.FindAsync(id)
          is Todo todo
          ? Results.Ok(new TodoDTO(todo)) 
          : Results.NotFound());

app.MapPut("/todoitems/{id}", async (int id, TodoDTO TodoDTO, TodoDB db) =>
 {
     var todo = await db.Todos.FindAsync(id);
     
     if(todo is null) return Results.NotFound();

     todo.Name = TodoDTO.Name;
     todo.IsCompleted = TodoDTO.IsCompleted;

     await db.SaveChangesAsync();

     return Results.NoContent();
 });
app.MapDelete("/todoitems/{id}", async (int id, TodoDB db) =>
 {

     if(await db.Todos.FindAsync(id) is Todo todo)
     {
         db.Todos.Remove(todo);
         await db.SaveChangesAsync();
         return Results.Ok(todo);
     }

   return Results.NotFound();
 });




app.Run();

class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsCompleted { get; set; }

    public string? Secret { get; set; }
}
class TodoDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsCompleted { get; set; }

    public TodoDTO() { }
    public TodoDTO(Todo todoitem) =>
        (Id, Name, IsCompleted) = (todoitem.Id, todoitem.Name, todoitem.IsCompleted);
}
class TodoDB: DbContext
{
    public TodoDB(DbContextOptions<TodoDB> options)
        : base(options) { }
    public DbSet<Todo> Todos => Set<Todo>();
}