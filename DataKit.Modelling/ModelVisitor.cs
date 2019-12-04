namespace DataKit.Modelling
{
	public interface IModelVisitor
	{
		IModelNode Visit(IModelNode modelNode);
		IModelNode VisitModel(IDataModel model);
		IModelNode VisitField<T>(IModelFieldOf<T> field);
		IModelNode VisitExtension(IModelNode node);
	}

	public abstract class ModelVisitor : IModelVisitor
	{
		public virtual IModelNode Visit(IModelNode modelNode)
		{
			if (modelNode != null)
				return modelNode.Accept(this);
			return null;
		}

		public virtual IModelNode VisitModel(IDataModel model)
		{
			return model;
		}

		public virtual IModelNode VisitField<T>(IModelFieldOf<T> field)
		{
			return field;
		}

		public virtual IModelNode VisitExtension(IModelNode node)
		{
			return node;
		}
	}
}
