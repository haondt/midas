using Haondt.Core.Extensions;
using Haondt.Web.Components;
using Haondt.Web.Core.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Mvc;
using SpendLess.Core.Extensions;
using SpendLess.NodeRed.Components;
using SpendLess.NodeRed.Models;
using SpendLess.NodeRed.Services;
using SpendLess.Web.Domain.Components;
using SpendLess.Web.Domain.Services;

namespace SpendLess.NodeRed.Controllers
{
    [Route("node-red")]
    public class NodeRedController(IComponentFactory componentFactory,
        INodeRedService nodeRed) : UIController
    {
        [HttpGet]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<NodeRed.Components.NodeRed>();
        }

        [HttpPost]
        public async Task<IResult> Post([FromForm(Name = "request-text")] string requestText)
        {
            var (status, result) = await nodeRed.SendToNodeRedRaw(requestText);

            var text = result.As(StringFormatter.TryPrettify);

            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = [
                    new ResponseText
                    {
                        Text = text.Or(""),
                    },
                    new ResponseHeader
                    {
                        Status = status,
                    },
                    new Toast {
                        Message = "Sent to Node Red successfully.",
                        Severity = Web.Domain.Models.ToastSeverity.Success
                    }
                ]
            });
        }

        [HttpGet("default-payload")]
        public Task<IResult> GetDefaultPayload()
        {
            return CreateCodeWindow(new SendToNodeRedRequestDto().ToString());
        }

        [HttpPost("prettify")]
        public Task<IResult> Prettify([FromForm(Name = "request-text")] string requestText)
        {
            return CreateCodeWindow(StringFormatter.TryPrettify(requestText));
        }

        private Task<IResult> CreateCodeWindow(string text)
        {
            return componentFactory.RenderComponentAsync(new CodeWindow
            {
                Text = text,
                Id = "request-text",
                Name = "request-text"
            });
        }
    }
}
