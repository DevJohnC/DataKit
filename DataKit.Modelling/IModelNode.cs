namespace DataKit.Modelling
{
	public interface IModelNode
	{
		/// <summary>
		/// Dispatches to the appropriate visit method on the provided <see cref="IModelVisitor"/>.
		/// </summary>
		/// <param name="visitor"></param>
		/// <returns></returns>
		IModelNode Accept(IModelVisitor visitor);
	}
}
