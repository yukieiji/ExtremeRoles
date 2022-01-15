using UnityEngine;

namespace ExtremeRoles.Module
{
    public class Arrow
    {
        public SpriteRenderer image;
        public GameObject Body;
        
        private Vector3 target;

        public Arrow(Color color)
        {
            Body = new GameObject("Arrow");
            Body.layer = 5;
            image = Body.AddComponent<SpriteRenderer>();
            image.sprite = Prefab.Arrow;
            image.color = color;
        }

        public void Update()
        {
            if (this.target == null) { this.target = Vector3.zero; }
            UpdateTarget();
        }

        public void SetColor(Color? color = null)
        {
            if (color.HasValue) { image.color = color.Value; };
        }

        public void UpdateTarget(Vector3? target=null)
        {
            if (Body == null) { return; }
            
            if (target.HasValue)
            {
                this.target = target.Value;
            }

            Camera main = Camera.main;

            Vector2 vector = this.target - main.transform.position;
            float num = vector.magnitude / (main.orthographicSize * 0.925f);
            image.enabled = ((double)num > 0.3);
            Vector2 vector2 = main.WorldToViewportPoint(this.target);

            if (between(vector2.x, 0f, 1f) && between(vector2.y, 0f, 1f))
            {
                Body.transform.position = this.target - (Vector3)vector.normalized * 0.6f;
                float num2 = Mathf.Clamp(num, 0f, 1f);
                Body.transform.localScale = new Vector3(num2, num2, num2);
            }
            else
            {
                Vector2 vector3 = new Vector2(
                    Mathf.Clamp(vector2.x * 2f - 1f, -1f, 1f),
                    Mathf.Clamp(vector2.y * 2f - 1f, -1f, 1f));
                float orthographicSize = main.orthographicSize;
                float num3 = main.orthographicSize * main.aspect;
                Vector3 vector4 = new Vector3(
                    Mathf.LerpUnclamped(0f, num3 * 0.88f, vector3.x),
                    Mathf.LerpUnclamped(0f, orthographicSize * 0.79f, vector3.y), 0f);
                Body.transform.position = main.transform.position + vector4;
                Body.transform.localScale = Vector3.one;
            }

            lookAt2d(Body.transform, this.target);
        }

        private bool between(float value, float min, float max)
        {
            return value > min && value < max;
        }

        private void lookAt2d(Transform transform, Vector3 target)
        {
            Vector3 vector = target - transform.position;
            vector.Normalize();
            float num = Mathf.Atan2(vector.y, vector.x);
            if (transform.lossyScale.x < 0f)
            {
                num += 3.1415927f;
            }
            transform.rotation = Quaternion.Euler(0f, 0f, num * 57.29578f);
        }
    }
}
