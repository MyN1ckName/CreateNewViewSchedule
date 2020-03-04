using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace CreateViewSchedule
{
	static class CreateNewViewSchedule
	{
		// Перевод милиметров в футы
		const double mmToft = 0.00328084;

		public static void CreateWallsSchedule(UIDocument uidoc)
		{
			Document doc = uidoc.Document;
			using (Transaction t = new Transaction(doc, "Create Schedule"))
			{
				t.Start();

				// Создание спецификации для стен
				ViewSchedule wallsSchedule =
						ViewSchedule.CreateSchedule(doc,
						new ElementId(BuiltInCategory.OST_Walls));

				// Добавление полей в спецификацию
				foreach (SchedulableField schedulableFields
					in GetSchedulableFields(doc))
				{
					wallsSchedule.Definition.AddField(schedulableFields);
				}

				// Назначение стадии
				SetPhaseByName(wallsSchedule, "KR-3");

				// Для каждого экземпляра = false
				wallsSchedule.Definition.IsItemized = false;

				// Вычесление итогов
				wallsSchedule.Definition.GetField(0).DisplayType =
					ScheduleFieldDisplayType.Totals;
				wallsSchedule.Definition.GetField(1).DisplayType =
					ScheduleFieldDisplayType.Totals;

				// Назначение ширины столбцам
				wallsSchedule.Definition.GetField(0).GridColumnWidth = 30 * mmToft;
				wallsSchedule.Definition.GetField(1).GridColumnWidth = 30 * mmToft;
				wallsSchedule.Definition.GetField(2).GridColumnWidth = 55 * mmToft;

				// Назначение выравнивания
				wallsSchedule.Definition.GetField(1).HorizontalAlignment =
					ScheduleHorizontalAlignment.Right;
				wallsSchedule.Definition.GetField(2).HorizontalAlignment =
					ScheduleHorizontalAlignment.Center;

				// Установка сортировки/групировки
				ScheduleSortGroupField sortGroupField =
					new ScheduleSortGroupField(
						wallsSchedule.Definition.GetField(2).FieldId,
						ScheduleSortOrder.Ascending);
				wallsSchedule.Definition.AddSortGroupField(sortGroupField);
				
				t.Commit();
				uidoc.ActiveView = wallsSchedule;
			}
		}

		// Создание полей спецификации
		static List<SchedulableField> GetSchedulableFields(Document doc)
		{
			FilteredElementCollector collector = new FilteredElementCollector(doc);

			Element element = collector.OfClass(typeof(Wall)).First();

			List<SchedulableField> schedulableFields = new List<SchedulableField>();

			// Список параметров из которых будут сделаны поля спецификации
			List<BuiltInParameter> parametersList = new List<BuiltInParameter>()
			{
				BuiltInParameter.CURVE_ELEM_LENGTH,
				BuiltInParameter.HOST_AREA_COMPUTED,
				BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM,
			};

			foreach (BuiltInParameter parameter in parametersList)
			{
				SchedulableField schedulableField = new SchedulableField()
				{
					ParameterId = element.get_Parameter(parameter).Id
				};
				schedulableFields.Add(schedulableField);
			}
			return schedulableFields;
		}

		// Назначение стадии для спецификации по заданному имени стадии
		static void SetPhaseByName(ViewSchedule viewSchedule, string phaseName)
		{
			Document doc = viewSchedule.Document;

			FilteredElementCollector phaseCollector = new FilteredElementCollector(doc);
			phaseCollector.OfClass(typeof(Phase));

			// Нужно намутить проверку на null,
			// хотя в Command есть cath {}
			Phase phase = (from item in phaseCollector
						   where item.Name.Equals(phaseName)
						   select item).First() as Phase;

			viewSchedule.get_Parameter(BuiltInParameter.VIEW_PHASE).Set(phase.Id);

			FilteredElementCollector phaseFilterCollector =
				new FilteredElementCollector(doc);
			phaseFilterCollector.OfClass(typeof(PhaseFilter));

			// Нужно намутить проверку на null,
			// хотя в Command есть cath {}
			PhaseFilter phaseFilter = (from item in phaseFilterCollector
									   where item.Name.Equals("Текущая и предыдущая")
									   select item).First() as PhaseFilter;

			viewSchedule.get_Parameter(BuiltInParameter.VIEW_PHASE_FILTER)
				.Set(phaseFilter.Id);
		}
	}
}