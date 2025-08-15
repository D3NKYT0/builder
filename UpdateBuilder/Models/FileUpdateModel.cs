using System.Xml.Serialization;

namespace UpdateBuilder.Models
{
	[XmlRoot("File")]
	public class FileUpdateModel
	{
		public string Name
		{
			get;
			set;
		}

		[XmlIgnore]
		public ModifyType ModifyType
		{
			get;
			set;
		}

		public long Size
		{
			get;
			set;
		}

		public string Hash
		{
			get;
			set;
		}

		public int Version
		{
			get;
			set;
		}
	}
}
