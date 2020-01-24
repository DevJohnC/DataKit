using DataKit.Modelling;
using System.Collections.Generic;

namespace DataKit.Mapping
{
	public class DataModelReaderNavigator<TReader, TDataModel, TField>
		where TReader : IDataModelReader<TDataModel, TField>
		where TDataModel : IDataModel<TField>
		where TField : IModelField
	{
		private readonly TReader _reader;

		private readonly List<TField> _positionStack = new List<TField>();

		public DataModelReaderNavigator(TReader reader)
		{
			_reader = reader;
		}

		public bool TrySeekToPosition(params TField[] fieldPath)
		{
			var commonRootDepth = SeekUpToCommonRoot(fieldPath);

			for (var i = commonRootDepth; i < fieldPath.Length; i++)
			{
				if (_reader.CanEnterMember(fieldPath[i]))
				{
					_reader.EnterMember(fieldPath[i]);
					_positionStack.Add(fieldPath[i]);
				}
				else
				{
					return false;
				}
			}

			return true;
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
					_reader.LeaveMember();
				}
				_positionStack.RemoveRange(i, removeCount);
			}

			return i;
		}
	}
}
