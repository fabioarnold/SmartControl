using Microsoft.AspNetCore.Mvc;
using SmartControlServer.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartControlServer
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class UpdatesController : ControllerBase
    {
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;

        public UpdatesController(IStoreContext storeContext, INotificationService notificationService)
        {
            _storeContext = storeContext;
            _notificationService = notificationService;
        }

        [HttpGet("{id}/{name}/{offset}")]
        public async Task<ActionResult<UpdateResponse>> GetAsync(Guid id, string name, ulong offset)
        {
            var requestId = Guid.NewGuid();
            var clientState = await GetClientStateAsync(requestId, id, name, offset, HttpContext.RequestAborted);
            return Ok(new UpdateResponse { State = clientState.State, Offset = clientState.Offset });
        }

        [HttpPost]
        public IActionResult PostAsync([FromBody] ClientCommand clientCommand)
        {
            _storeContext.SetClientState(clientCommand.Id, clientCommand.Command);
            _notificationService.Notify(clientCommand.Id);
            return Ok();
        }

        private async Task<StoreContext.Client> GetClientStateAsync(Guid requestId, Guid id, string name, ulong offset, CancellationToken cancellationToken)
        {
            var client = _storeContext.GetClientState(id, name);
            if (client.Offset == offset) // End of queue -> get notified when new commands arrive
            {
                var tcs = new TaskCompletionSource(cancellationToken);
                cancellationToken.Register(() => { tcs.SetCanceled(); }); // Propagate cancellation
                _notificationService.Subscribe(requestId, id, tcs);
                try
                {
                    await tcs.Task; // Wait for notification updates
                    _notificationService.Unsubscribe(requestId, id);
                    client = _storeContext.GetClientState(id, name);
                }
                catch (TaskCanceledException)
                {
                    _notificationService.Unsubscribe(requestId, id);
                }
            }

            return client;
        }
    }
}
