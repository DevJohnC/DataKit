using DataKit.Modelling;
using System;

namespace DataKit.Mapping
{
	public delegate void MappingDelegate<TSourceModel, TSourceField, TTargetModel, TTargetField>(
		IDataModelReader<TSourceModel, TSourceField> sourceReader,
		IDataModelWriter<TTargetModel, TTargetField> targetWriter)
		where TSourceModel : IDataModel<TSourceField>
		where TSourceField : IModelField
		where TTargetModel : IDataModel<TTargetField>
		where TTargetField : IModelField;

	/// <summary>
	/// A mapping between two models.
	/// </summary>
	public abstract class Mapping
	{
		public IDataModel SourceModel { get; }

		public IDataModel TargetModel { get; }

		protected Mapping(IDataModel sourceModel, IDataModel targetModel)
		{
			SourceModel = sourceModel;
			TargetModel = targetModel;
		}
	}

	/// <summary>
	/// A mapping between two models.
	/// </summary>
	public class Mapping<TSourceModel, TSourceField, TTargetModel, TTargetField> : Mapping
		where TSourceModel : IDataModel<TSourceField>
		where TSourceField : IModelField
		where TTargetModel : IDataModel<TTargetField>
		where TTargetField : IModelField
	{
		public new TSourceModel SourceModel { get; }

		public new TTargetModel TargetModel { get; }

		private MappingDelegate<TSourceModel, TSourceField, TTargetModel, TTargetField> _mappingDelegate;

		public Mapping(TSourceModel sourceModel, TTargetModel targetModel, MappingDelegate<TSourceModel, TSourceField, TTargetModel, TTargetField> mappingDelegate) :
			base(sourceModel, targetModel)
		{
			SourceModel = sourceModel;
			TargetModel = targetModel; 
			_mappingDelegate = mappingDelegate ?? throw new ArgumentNullException(nameof(mappingDelegate));
		}

		public void Run(IDataModelReader<TSourceModel, TSourceField> sourceReader, IDataModelWriter<TTargetModel, TTargetField> targetWriter)
			=> _mappingDelegate(sourceReader, targetWriter);
	}
}
