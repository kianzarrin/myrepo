using System;
using System.Collections.Generic;
using ColossalFramework;
using UnityEngine;
using ColossalFramework.Math;
using Elektrix.Tools.Extensions;

namespace ResolveOverlaps
{
	// Token: 0x02000007 RID: 7
	public partial class ResolveOverlapsTool : ToolBase
	{
		// Token: 0x06000025 RID: 37 RVA: 0x00002E08 File Offset: 0x00001008
		public bool DissolveNode(ushort nodeID)
		{
			NetNode node = this.GetNode(nodeID);
			bool result;
			if (node.CountSegments() != 2)
			{
				result = this.ThrowError("To dissolve a node, you must have exactly two connecting segments.");
			}
			else
			{
				ushort num = 0;
				ushort num2 = 0;
				for (int i = 0; i < 8; i++)
				{
					if (node.GetSegment(i) > 0)
					{
						if (num != 0)
						{
							num2 = node.GetSegment(i);
							break;
						}
						num = node.GetSegment(i);
					}
				}
				if (num != 0 && num2 != 0)
				{
					NetSegment segment;
					NetSegment segment2;
					try
					{
						segment = this.GetSegment(num);
						segment2 = this.GetSegment(num2);
					}
					catch (Exception)
					{
						return true;
					}
					ushort num3 = (segment.m_startNode == nodeID) ? segment.m_endNode : segment.m_startNode;
					Vector3 vector = (segment.m_startNode == nodeID) ? segment.m_endDirection : segment.m_startDirection;
					ushort num4 = (segment2.m_startNode == nodeID) ? segment2.m_endNode : segment2.m_startNode;
					Vector3 vector2 = (segment2.m_startNode == nodeID) ? segment2.m_endDirection : segment2.m_startDirection;
					
          //Here is the fix:
          bool flag = num3 == segment.m_startNode;
					bool flag2 = (segment.m_flags & NetSegment.Flags.Invert) > NetSegment.Flags.None;
					ushort num5;
					if (flag ^ flag2)
					{
						this.Manager.CreateSegment(out num5, ref Singleton<SimulationManager>.instance.m_randomizer, segment.Info, num3, num4, vector, vector2, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false);
					}
					else
					{
						this.Manager.CreateSegment(out num5, ref Singleton<SimulationManager>.instance.m_randomizer, segment.Info, num4, num3, vector2, vector, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, false);
					}
          //end of fix
          
					Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
					this.NetworkSkinsFixNewPrefab(node.GetSegment(0), num5, segment.Info);
					this.Manager.ReleaseNode(nodeID);
					return num5 > 0;
				}
				result = this.ThrowError("Invalid segment detected.");
			}
			return result;
		}
		
				// Token: 0x06000023 RID: 35 RVA: 0x00002A64 File Offset: 0x00000C64
		public bool NodeInsertion(ushort segmentID, float cut)
		{
			Bezier3 bezier = default(Bezier3);
			NetSegment segment = this.GetSegment(segmentID);
			bezier.a = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_startNode].m_position;
			bezier.d = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_endNode].m_position;
			bool smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_startNode].m_flags & NetNode.Flags.Middle) > NetNode.Flags.None;
			bool smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[(int)segment.m_endNode].m_flags & NetNode.Flags.Middle) > NetNode.Flags.None;
			Vector3 b;
			Vector3 c;
			NetSegment.CalculateMiddlePoints(bezier.a, segment.m_startDirection, bezier.d, segment.m_endDirection, smoothStart, smoothEnd, out b, out c);
			bezier.b = b;
			bezier.c = c;
			Vector3 position = bezier.Position(cut);
			ushort startNode = segment.m_startNode;
			Vector3 startDirection = segment.m_startDirection;
			Vector3 endDirection = -bezier.Tangent(cut).normalized;
			ushort endNode = segment.m_endNode;
			Vector3 endDirection2 = segment.m_endDirection;
			Vector3 normalized = bezier.Tangent(cut).normalized;
			ushort num;
			this.Manager.CreateNode(out num, ref Singleton<SimulationManager>.instance.m_randomizer, segment.Info, position, Singleton<SimulationManager>.instance.m_currentBuildIndex);
			Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
			bool invert = (segmentID.ToSegment().m_flags & NetSegment.Flags.Invert) > NetSegment.Flags.None;
			ushort num2;
			this.Manager.CreateSegment(out num2, ref Singleton<SimulationManager>.instance.m_randomizer, segment.Info, startNode, num, startDirection, endDirection, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);
			Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
			ushort num3;
			this.Manager.CreateSegment(out num3, ref Singleton<SimulationManager>.instance.m_randomizer, segment.Info, num, endNode, normalized, endDirection2, Singleton<SimulationManager>.instance.m_currentBuildIndex, Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);
			Singleton<SimulationManager>.instance.m_currentBuildIndex += 1u;
			this.Manager.UpdateNode(num);
			this.Manager.UpdateSegment(num2);
			this.Manager.UpdateSegment(num3);
			this.NetworkSkinsFixNewPrefab(segmentID, num2, segment.Info);
			this.NetworkSkinsFixNewPrefab(segmentID, num3, segment.Info);
			this.Manager.ReleaseSegment(segmentID, true);
			this.m_newSeg1 = num2;
			this.m_newSeg2 = num3;
			return true;
		}
	}
}
