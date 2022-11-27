using TodoApi.Controllers;
using TodoApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TodoApi.Tests;

#nullable disable
public class TodoItemsControllerTests
{
  private DbContextOptions<TodoContext> DatabaseContextOptions()
  {
    return new DbContextOptionsBuilder<TodoContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
  }

  private void PopulateDatabaseContext(TodoContext context)
  {
    context.TodoItem.Add(new Models.TodoItem { Id = 1, Name = "Task 1", IsComplete = true });
    context.TodoItem.Add(new Models.TodoItem { Id = 2, Name = "Task 2", IsComplete = false });
    context.SaveChanges();
  }

  [Fact]
  public async Task GetTodoItems_WhenCalled_ReturnsTodoItemsList()
  {
    using (var context = new TodoContext(DatabaseContextOptions()))
    {
      PopulateDatabaseContext(context);

      var controller = new TodoItemsController(context);

      var result = await controller.GetTodoItems();

      Assert.IsType<OkObjectResult>(result.Result);
      Assert.Equal(
        2,
        ((result.Result as OkObjectResult).Value as IList<TodoItem>).Count
      );
    }
  }

  [Fact]
  public async Task GetTodoItem_WhenCalled_ReturnsTodoItemById()
  {
    using (var context = new TodoContext(DatabaseContextOptions()))
    {
      PopulateDatabaseContext(context);

      var controller = new TodoItemsController(context);

      var result = await controller.GetTodoItem(1);

      Assert.IsType<OkObjectResult>(result.Result);
      Assert.Equal(
        1,
        ((result.Result as OkObjectResult).Value as TodoItem).Id
      );
    }
  }

  [Fact]
  public async Task PutTodoItem_WhenTodoItemIdDoesntMatch_ReturnsBadRequest()
  {
    using (var context = new TodoContext(DatabaseContextOptions()))
    {
      PopulateDatabaseContext(context);

      var controller = new TodoItemsController(context);

      var todoItem = await context.TodoItem.Where(x => x.Id == 2).FirstAsync();
      var result = await controller.PutTodoItem(1, todoItem);

      Assert.IsType<BadRequestResult>(result);
    }
  }

  [Fact]
  public async Task PutTodoItem_WhenTodoItemDoesntExist_ReturnsBadRequest()
  {
    using (var context = new TodoContext(DatabaseContextOptions()))
    {
      PopulateDatabaseContext(context);

      var controller = new TodoItemsController(context);

      var result = await controller.PutTodoItem(3, new TodoItem { Id = 3});

      Assert.IsType<NotFoundResult>(result);
    }
  }

  [Fact]
  public async Task PutTodoItem_WhenCalled_UpdatesTheTodoItem()
  {
    using (var context = new TodoContext(DatabaseContextOptions()))
    {
      PopulateDatabaseContext(context);

      var controller = new TodoItemsController(context);

      var todoItem = await context.TodoItem.Where(x => x.Id == 2).FirstAsync();
      var result = await controller.PutTodoItem(todoItem.Id, todoItem);

      Assert.IsType<NoContentResult>(result);
    }
  }

  [Fact]
  public async Task PostTodoItem_WhenCalled_CreatesTodoItem()
  {
    using (var context = new TodoContext(DatabaseContextOptions()))
    {
      PopulateDatabaseContext(context);

      var controller = new TodoItemsController(context);

      var todoItem = new TodoItem { Name = "Task 3", IsComplete = false };
      var result = await controller.PostTodoItem(todoItem);

      Assert.IsType<CreatedAtActionResult>(result.Result);
      Assert.Equal(
        3,
        context.TodoItem.Count()
      );
    }
  }

  [Fact]
  public async Task DeleteTodoItem_WhenCalled_RemovesTodoItem()
  {
    using (var context = new TodoContext(DatabaseContextOptions()))
    {
      PopulateDatabaseContext(context);

      var controller = new TodoItemsController(context);

      var result = await controller.DeleteTodoItem(2);

      Assert.IsType<NoContentResult>(result);
      Assert.Equal(
        1,
        context.TodoItem.Count()
      );
    }
  }
}