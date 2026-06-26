using AlpineSkiHouse.Data;
using AlpineSkiHouse.Events;
using AlpineSkiHouse.Models;
using AlpineSkiHouse.Web.Command;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AlpineSkiHouse.Web.Handlers
{
    public class ActivatePassHandler : IRequestHandler<ActivatePass, int>
    {
        private readonly PassContext _passContext;
        private readonly IMediator _mediator;

        public ActivatePassHandler(PassContext passContext, IMediator mediator)
        {
            _passContext = passContext;
            _mediator = mediator;
        }

        public async Task<int> Handle(ActivatePass message, CancellationToken cancellationToken)
        {
            PassActivation activation = new PassActivation
            {
                PassId = message.PassId,
                ScanId = message.ScanId
            };
            _passContext.PassActivations.Add(activation);
            await _passContext.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new PassActivated { PassActivationId = activation.Id }, cancellationToken);

            return activation.Id;
        }
    }
}
