using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicService.Objects
{
    public class UserProfile
    {

        public string UserName { get; set; }

        public List<UserTrace> UserTraces { get; set; }

    }

    public class UserTrace
    {

        public string Device { get; set; }

        public string IP { get; set; }

        public DateTime Time { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;

            var one = (UserTrace)obj;
            if (this.Device == one.Device)
                return true;
            else
                return false;
        }

        public static bool operator ==(UserTrace leftHandSide, UserTrace rightHandSide)
        {
            if (ReferenceEquals(leftHandSide, null))
                return ReferenceEquals(rightHandSide, null);
            return (leftHandSide.Equals(rightHandSide));
        }

        public static bool operator !=(UserTrace leftHandSide, UserTrace rightHandSide)
        {
            return !(leftHandSide == rightHandSide);
        }

        public override int GetHashCode()
        {
            return Device.GetHashCode();
        }

    }

}
