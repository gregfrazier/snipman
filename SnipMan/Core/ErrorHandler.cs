namespace SnipMan.Core
{
    class ErrorHandler
    {
        public ErrorHandler(string errorMsg, string stackTrace){
            System.Windows.Forms.MessageBox.Show(errorMsg, Config.ApplicationName + " Error");
        }
    }
}
