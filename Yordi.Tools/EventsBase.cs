using System.Runtime.CompilerServices;

namespace Yordi.Tools
{
    public delegate void MyMessage(string msg, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0, [CallerFilePath] string path = "");
    public delegate void MyProgress(float progressValue);
    public delegate void MyRows(float progressMax);
    public delegate void MyError(string erro, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0, [CallerFilePath] string path = "");
    public delegate void MyException(Exception erro, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0, [CallerFilePath] string path = "");
    public delegate void BoolChanged(bool valor);
    public delegate void BoolChangedDelegate(object sender, bool valor);
}
