using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Text;

namespace LogicService.Application
{
    public class ApplicationVersion
    {
        public ushort Major { get; private set; }

        public ushort Minor { get; private set; }

        public ushort Build { get; private set; }

        public ushort Revision { get; private set; }

        public ApplicationVersion(ushort major, ushort minor, ushort build, ushort revision)
        {
            this.Major = major;
            this.Minor = minor;
            this.Build = build;
            this.Revision = revision;
        }

        public ApplicationVersion(string version)
        {
            string[] vs = version.Split('.');
            this.Major = Convert.ToUInt16(vs[0]);
            this.Minor = Convert.ToUInt16(vs[1]);
            this.Build = Convert.ToUInt16(vs[2]);
            this.Revision = Convert.ToUInt16(vs[3]);
        }

        public static ApplicationVersion Parse(string version)
        {
            return new ApplicationVersion(version);
        }

        public static ApplicationVersion CurrentVersion()
        {
            return new ApplicationVersion(SystemInformation.ApplicationVersion.Major, SystemInformation.ApplicationVersion.Minor,
                SystemInformation.ApplicationVersion.Build, SystemInformation.ApplicationVersion.Revision);
        }

        public static bool operator >(ApplicationVersion version1, ApplicationVersion version2)
        {
            if (version1.Major > version2.Major)
                return true;
            else if (version1.Major < version2.Major)
                return false;
            else
            {
                if (version1.Minor > version2.Minor)
                    return true;
                else if (version1.Minor < version2.Minor)
                    return false;
                else
                    return false;
            }
        }

        public static bool operator <(ApplicationVersion version1, ApplicationVersion version2)
        {
            if (version1.Major > version2.Major)
                return true;
            else if (version1.Major < version2.Major)
                return false;
            else
            {
                if (version1.Minor > version2.Minor)
                    return true;
                else if (version1.Minor < version2.Minor)
                    return false;
                else
                    return false;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Major.ToString());
            builder.Append(".");
            builder.Append(Minor.ToString());
            builder.Append(".");
            builder.Append(Build.ToString());
            builder.Append(".");
            builder.Append(Revision.ToString());
            return builder.ToString();
        }
    }
}
