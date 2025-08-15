using System.Xml.Serialization;

namespace UpdateBuilder.Models
{
	[XmlRoot("UpdateInfo")]
	public class UpdateInfoModel
	{
		public int Version
		{
			get;
			set;
		}

		public FolderModel Folder
		{
			get;
			set;
		}
	}
}
