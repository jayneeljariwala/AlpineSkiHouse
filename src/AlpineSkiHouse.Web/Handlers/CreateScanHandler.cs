using AlpineSkiHouse.Data;
using AlpineSkiHouse.Events;
using AlpineSkiHouse.Models;
using AlpineSkiHouse.Services;
using AlpineSkiHouse.Web.Command;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AlpineSkiHouse.Handlers
{
    public class CreateScanHandler : IRequestHandler<CreateScan, int>
    {
        private readonly PassContext _passContext;
        private readonly IDateService _dateService;
        private readonly IMediator _mediator;

        public CreateScanHandler(PassContext passContext, IDateService dateService, IMediator mediator)
        {
            _dateService = dateService;
            _passContext = passContext;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateScan message, CancellationToken cancellationToken)
        {
            var scan = new Scan
            {
                CardId = message.CardId,
                LocationId = message.LocationId,
                DateTime = _dateService.Now()
            };
            _passContext.Scans.Add(scan);
            await _passContext.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new CardScanned { ScanId = scan.Id }, cancellationToken);            
            return scan.Id;
        }
    }
}
