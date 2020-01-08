using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnnotationTool
{
    class SystemEnvironmentChecker
    {
        public static void Check()
        {
            var version = Environment.Version;
            if (version.Major == 4 && version.Minor == 0 && version.Build == 30319 && version.Revision < 418)
            {
                throw new ApplicationException("该版本的.Net Runtime不满足最低运行要求，请安装目录下的NDP452-KB2901907-x86-x64-AllOS-ENU.exe并重新启动系统.");
            }
        }
    }
}
