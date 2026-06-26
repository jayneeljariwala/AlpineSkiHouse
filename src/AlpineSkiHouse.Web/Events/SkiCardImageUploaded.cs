using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlpineSkiHouse.Events
{
    public class SkiCardImageUploaded : INotification
    {
        public string FileName { get; set; }
    }
}
