using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace CreateViewSchedule
{
	[TransactionAttribute(TransactionMode.Manual)]
	[RegenerationAttribute(RegenerationOption.Manual)]
	public class Command : IExternalCommand
	{
		UIDocument uidoc;

		public Result Execute(ExternalCommandData commandData,
			ref string messege,
			ElementSet elements)
		{
			uidoc = commandData.Application.ActiveUIDocument;
			try
			{
				CreateNewViewSchedule.CreateWallsSchedule(uidoc);

				return Result.Succeeded;
			}

			catch (Autodesk.Revit.Exceptions.OperationCanceledException)
			{
				return Result.Cancelled;
			}
			catch (Exception ex)
			{
				messege = ex.Message;
				return Result.Failed;
			}
		}
	}
}