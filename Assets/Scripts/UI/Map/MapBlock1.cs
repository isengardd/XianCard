/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UI.Map
{
	public partial class MapBlock1 : GComponent
	{
		public MapNodeCom node1;
		public MapNodeCom node2;
		public MapNodeCom node3;
		public MapNodeCom node4;
		public MapNodeCom node5;
		public MapNodeCom node6;
		public MapNodeCom node7;
		public MapNodeCom node8;
		public MapNodeCom node9;
		public MapNodeCom node10;
		public MapNodeCom node11;

		public const string URL = "ui://9zqi84sykp7y6";

		public static MapBlock1 CreateInstance()
		{
			return (MapBlock1)UIPackage.CreateObject("Map","MapBlock1");
		}

		public MapBlock1()
		{
		}

		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			node1 = (MapNodeCom)this.GetChild("node1");
			node2 = (MapNodeCom)this.GetChild("node2");
			node3 = (MapNodeCom)this.GetChild("node3");
			node4 = (MapNodeCom)this.GetChild("node4");
			node5 = (MapNodeCom)this.GetChild("node5");
			node6 = (MapNodeCom)this.GetChild("node6");
			node7 = (MapNodeCom)this.GetChild("node7");
			node8 = (MapNodeCom)this.GetChild("node8");
			node9 = (MapNodeCom)this.GetChild("node9");
			node10 = (MapNodeCom)this.GetChild("node10");
			node11 = (MapNodeCom)this.GetChild("node11");
		}
	}
}