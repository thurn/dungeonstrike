using UnityEngine;
using System.Collections.Generic;

namespace DungeonStrike
{
    public class LinkService : MonoBehaviour
    {
        private static LinkService _instance;
        public static LinkService Instance
        {
            get { return _instance ?? (_instance = FindObjectOfType<LinkService>()); }
        }
        private IList<Link> _links = new List<Link>();

		public void AddLink(Link link)
		{
            _links.Add(link);
        }
    }
}