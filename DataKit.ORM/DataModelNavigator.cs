using DataKit.Mapping;
using DataKit.Mapping.Binding;
using DataKit.Modelling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataKit.ORM
{
	internal class DataModelReaderNavigator<TDataModel, TField>
		where TDataModel : IDataModel<TField>
		where TField : IModelField
	{
		private readonly List<string> _currentPosition = new List<string>();

		public IDataModelReader<TDataModel, TField> Reader { get; }

		private readonly IReadOnlyList<string> _topPosition;
		private readonly int _topPositionDepth;

		public IEnumerable<string> CurrentPosition => _currentPosition;

		public DataModelReaderNavigator(IDataModelReader<TDataModel, TField> reader,
			IReadOnlyList<string> topPosition = null)
		{
			Reader = reader;
			_topPosition = topPosition;
			_topPositionDepth = topPosition?.Count ?? 0;
		}

		public bool TryPrepareForRead(ModelFieldBindingSource<TField> bindingSource)
		{
			var desiredGraphPosition = bindingSource.Path.Take(bindingSource.Path.Length - 1);
			if (desiredGraphPosition.SequenceEqual(_currentPosition))
				//  no-op if already in position to read
				return true;

			//  todo: don't rely on exceptions here to control flow, it's naughty
			try
			{
				//  todo: optimize graph navigation
				//    for now we just seek to the top of the graph and descend back to where we want to be
				//    this can be done in fewer steps if the desired path and current path
				//    share a common root
				SeekToTop();
				SeekTo(desiredGraphPosition);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private void SeekToTop()
		{
			while (_currentPosition.Count > _topPositionDepth)
			{
				Reader.LeaveMember();
				_currentPosition.RemoveAt(_currentPosition.Count - 1);
			}
		}

		private void SeekTo(IEnumerable<string> path)
		{
			var fields = Reader.Model.Fields;
			foreach (var segment in path.Skip(_topPositionDepth))
			{
				if (fields == null)
					throw new InvalidOperationException("Model depth too shallow to navigate to desired path.");

				var field = fields.FirstOrDefault(q => q.FieldName == segment);
				if (field == null)
					throw new InvalidOperationException("Path isn't valid for model.");
				Reader.EnterMember(field);
				_currentPosition.Add(field.FieldName);

				//  this is a bit of a mess, optionally we could use a generic contraint to garuantee the type of TField if we see issues in use
				fields = field.FieldModel?.Fields.OfType<TField>().ToArray();
			}
		}
	}
}
