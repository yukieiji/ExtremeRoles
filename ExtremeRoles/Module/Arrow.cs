using UnityEngine;

using ExtremeRoles.Module.CustomMonoBehaviour;

namespace ExtremeRoles.Module
{

    public sealed class Arrow
    {
        public GameObject Main
        {
            get => this.body;
        }

        public Vector3 Target { get; private set; }

        private const float xzMaxSize = 0.4f;
        private const float yMaxSize = 0.525f;

        private GameObject body;
        private SpriteRenderer image;
        private ArrowBehaviour arrowBehaviour;
        private static readonly Vector3 defaultPos = new Vector3(100.0f, 100.0f, 100.0f);

        public Arrow(Color color)
        {            
            this.body = new GameObject("Arrow");

            this.body.layer = 5;
            this.image = this.body.AddComponent<SpriteRenderer>();

            if (Prefab.Arrow != null)
            {
                this.image.sprite = Prefab.Arrow;
            }
            this.image.color = color;
            this.arrowBehaviour = this.body.AddComponent<ArrowBehaviour>();
            this.arrowBehaviour.image = this.image;

            Resizeer resizer = this.body.AddComponent<Resizeer>();
            resizer.SetScale(xzMaxSize, yMaxSize, xzMaxSize);
            this.Target = defaultPos;
        }

        public void Update()
        {
            if (Prefab.Arrow != null && this.image == null)
            {
                this.image.sprite = Prefab.Arrow;
            }
            if (this.Target == defaultPos)
            { 
                this.Target = Vector3.zero;
            }
            UpdateTarget();
        }

        public void SetColor(Color? color = null)
        {
            if (color.HasValue) { this.image.color = color.Value; };
        }

        public void UpdateTarget(Vector3? target=null)
        {
            if (this.body == null) { return; }
            
            if (target.HasValue)
            {
                this.Target = target.Value;
            }

            this.arrowBehaviour.target = this.Target;
            this.arrowBehaviour.Update();
        }

        public void Clear()
        {
            Object.Destroy(this.body);
        }
        public void SetActive(bool active)
        {
            if (this.body != null)
            {
                this.body.SetActive(active);
            }
        }
    }
}
