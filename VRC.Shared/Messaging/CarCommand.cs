using System;

namespace VRC.Shared.Messaging
{
    public class CarCommand
    {
        public int CarNumber { get; set; }
        public int Throttle { get; set; }
        public int Direction { get; set; }
    }
}
