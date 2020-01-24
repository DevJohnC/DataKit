using DataKit.Modelling;
using System.Collections.Generic;

namespace DataKit.Mapping
{
	public class DataModelWriterNavigator<TWriter, TDataModel, TField>
		where TWriter : IDataModelWriter<TDataModel, TField>
		where TDataModel : IDataModel<TField>
		where TField : IModelField
	{
		private readonly TWriter _writer;

		private readonly List<TField> _positionStack = new List<TField>();

		public DataModelWriterNavigator(TWriter writer)
		{
			_writer = writer;
		}

		public void SeekToPosition(params TField[] fieldPath)
		{
			var commonRootDepth = SeekUpToCommonRoot(fieldPath);

			for (var i = commonRootDepth; i < fieldPath.Length; i++)
			{
				_writer.EnterMember(fieldPath[i]);
				_positionStack.Add(fieldPath[i]);
			}
		}

		public void SeekToTop()
		{
			for (var i = 0; i < _positionStack.Count; i++)
			{
				_writer.LeaveMember();
			}
			_positionStack.Clear();
		}

		private int SeekUpToCommonRoot(params TField[] fieldPath)
		{
			var i = 0;

			foreach (var currentPathSegment in _positionStack)
			{
				if (fieldPath.Length < i + 1 || currentPathSegment.FieldName != fieldPath[i].FieldName)
					break;

				i++;
			}

			var removeCount = _positionStack.Count - i;
			if (removeCount > 0)
			{
				for (var j = 0; j < removeCount; j++)
				{
					_writer.LeaveMember();
				}
				_positionStack.RemoveRange(i, removeCount);
			}

			return i;
		}
	}
}
