using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace AnnotationTool
{
    class ExceptionHandler
    {
        public static string ErrorStringFormater(Exception e)
        {
            var currentException = e;
            string errorMessage = "";
            while (true)
            {
                errorMessage += $"异常信息：{currentException.Message}\n" +
                                $"调用栈：\n{currentException.StackTrace}";
                if (currentException.InnerException != null)
                {
                    errorMessage += "\nInnerException:\n";
                    currentException = currentException.InnerException;
                }
                else
                {
                    break;
                }
            }

            return errorMessage;
        }

        public void UnexpectedExceptionHanlder(Exception e)
        {
            string errorMessage = "捕捉到未知的异常，请向程序作者报告此次错误，程序将退出。\n";
            errorMessage += ErrorStringFormater(e);

            MessageBox.Show(errorMessage);
        }
    }
}
