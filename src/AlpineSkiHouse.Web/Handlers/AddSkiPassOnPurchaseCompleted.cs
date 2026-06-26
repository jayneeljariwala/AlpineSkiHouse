using AlpineSkiHouse.Data;
using AlpineSkiHouse.Events;
using AlpineSkiHouse.Models;
using AlpineSkiHouse.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AlpineSkiHouse.Handlers
{
    public class AddSkiPassOnPurchaseCompleted : INotificationHandler<PurchaseCompleted>
    {
        private readonly PassContext _passContext;
        private readonly IMediator _bus;
        private readonly IDateService _dateService;

        public AddSkiPassOnPurchaseCompleted(PassContext passContext, IMediator bus, IDateService dateService)
        {
            _dateService = dateService;
            _passContext = passContext;
            _bus = bus;
        }

        public async Task Handle(PurchaseCompleted notification, CancellationToken cancellationToken)
        {
            var newPasses = new List<Pass>();
            foreach (var passPurchase in notification.Passes)
            {
                Pass pass = new Pass
                {
                    CardId = passPurchase.CardId,
                    CreatedOn = _dateService.Now(),
                    PassTypeId = passPurchase.PassTypeId
                };
                newPasses.Add(pass);
            }

            _passContext.Passes.AddRange(newPasses);
            await _passContext.SaveChangesAsync(cancellationToken);

            foreach (var newPass in newPasses)
            {
                var passAddedEvent = new PassAdded
                {
                    PassId = newPass.Id,
                    PassTypeId = newPass.PassTypeId,
                    CardId = newPass.CardId,
                    CreatedOn = newPass.CreatedOn
                };
                await _bus.Publish(passAddedEvent, cancellationToken);
            }
        }
    }
}
