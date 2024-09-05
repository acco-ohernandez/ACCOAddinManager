using System.Diagnostics;
using System.IO;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
namespace ACCOAddinManager
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_Run_ACCOAddinManager : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here
            //TaskDialog.Show("ACCOAddinManager", $"Hello from {MethodBase.GetCurrentMethod().DeclaringType?.FullName}!");

            //var flagFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "ACCOAddinManager", "ACCOAddinManager.flag");
            var Userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Local\Temp";
            string nameSpace = this.GetType().Namespace;
            //var flagDir = Path.Combine(Path.GetTempPath(), nameSpace);
            var flagFile = Path.Combine(Userprofile, nameSpace, $"{nameSpace}.flag");


            if (File.Exists(flagFile))
            {
                File.Delete(flagFile);
                ACCOAddinManager.App app = new ACCOAddinManager.App();
                app.WriteLog($"Flag file deleted on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}, The ACCOAddinManager will run on next Revit startup.", App.LogLevel.Info);
            }

            AskUserToRestartRevit();
            //TaskDialog.Show("ACCOAddinManager", $"The ACCOAddinManager will check for updates next time you start Revit.");

            return Result.Succeeded;
        }
        // write method to restart revit
        private void RestartRevit()
        {
            try
            {
                // Get the path to the Revit executable
                string revitPath = Process.GetCurrentProcess().MainModule.FileName;

                Process.Start(new ProcessStartInfo
                {
                    FileName = revitPath,
                    UseShellExecute = true,
                    Verb = "runas" // Optional: Use "runas" to run the process with elevated privileges
                });

                // Terminate the current process (Revit)
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                // Log the error or display a message box
                TaskDialog.Show("Error", $"Failed to restart Revit: {ex.Message}");
            }
        }

        // Method to ask the user if they want to restart Revit
        private void AskUserToRestartRevit()
        {
            TaskDialogResult result = TaskDialog.Show("ACCOAddinManager",
                "The ACCOAddinManager will check for updates next time you start Revit. \nDo you want to restart Revit now?",
                TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

            if (result == TaskDialogResult.Yes)
            {
                RestartRevit();
            }
        }


        //internal static PushButtonData GetButtonData()
        //{
        //    // use this method to define the properties for this command in the Revit ribbon
        //    string buttonInternalName = "btnCommand1";
        //    string buttonTitle = "Button 1";
        //    string? methodBase = MethodBase.GetCurrentMethod().DeclaringType?.FullName;

        //    if (methodBase == null)
        //    {
        //        throw new InvalidOperationException("MethodBase.GetCurrentMethod().DeclaringType?.FullName is null");
        //    }
        //    else
        //    {
        //        Common.ButtonDataClass myButtonData1 = new Common.ButtonDataClass(
        //            buttonInternalName,
        //            buttonTitle,
        //            methodBase,
        //            Properties.Resources.Blue_32,
        //            Properties.Resources.Blue_16,
        //            "This is a tooltip for Button 1");

        //        return myButtonData1.Data;
        //    }
        //}
    }

}
