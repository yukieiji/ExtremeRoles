using UnityEngine;

namespace ExtremeRoles.Module
{
    public class Arrow
    {   
        private Vector3 target;
        private SpriteRenderer image;
        private GameObject body;

        public Arrow(Color color)
        {
            this.body = new GameObject("Arrow");
            this.body.layer = 5;
            this.image = this.body.AddComponent<SpriteRenderer>();
            this.image.sprite = Prefab.Arrow;
            this.image.color = color;
        }

        public void Update()
        {
            if (this.target == null) { this.target = Vector3.zero; }
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
                this.target = target.Value;
            }

            Camera main = Camera.main;

            Vector2 vector = this.target - main.transform.position;
            float num = vector.magnitude / (main.orthographicSize * 0.75f);
            this.image.enabled = ((double)num > 0.3);
            Vector2 vector2 = main.WorldToViewportPoint(this.target);

            if (between(vector2.x, 0f, 1f) && between(vector2.y, 0f, 1f))
            {
                this.body.transform.position = this.target - (Vector3)vector.normalized * 0.6f;
                float num2 = Mathf.Clamp(num, 0f, 1f);
                this.body.transform.localScale = new Vector3(num2, num2, num2);
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
                this.body.transform.position = main.transform.position + vector4;
                this.body.transform.localScale = Vector3.one;
            }

            lookAt2d(this.body.transform, this.target);
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
