using UnityEngine;

namespace MonsterSurvivor.Combat
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class ShotTracer : MonoBehaviour
    {
        private SpriteRenderer beam;
        private Sprite beamSprite;
        private float remaining;

        private void Awake()
        {
            SpriteRenderer source = GetComponent<SpriteRenderer>();
            var beamObject = new GameObject("Shot Tracer");
            beamObject.transform.SetParent(transform, false);
            beam = beamObject.AddComponent<SpriteRenderer>();
            Texture2D texture = Texture2D.whiteTexture;
            beamSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), texture.width, 0, SpriteMeshType.FullRect);
            beam.sprite = beamSprite;
            beam.sharedMaterial = source.sharedMaterial;
            beam.sortingLayerID = source.sortingLayerID;
            beam.sortingOrder = source.sortingOrder + 2;
            beam.color = new Color(1f, 0.8f, 0.35f);
            beam.enabled = false;
        }

        public void Show(Vector2 start, Vector2 end)
        {
            Vector2 delta = end - start;
            beam.transform.position = new Vector3((start.x + end.x) * 0.5f,
                (start.y + end.y) * 0.5f, transform.position.z);
            beam.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
            beam.transform.localScale = new Vector3(delta.magnitude, 0.04f, 1f);
            remaining = 0.08f;
            beam.enabled = true;
        }

        private void Update()
        {
            if (!beam.enabled)
                return;
            remaining -= Time.deltaTime;
            if (remaining <= 0f)
                beam.enabled = false;
        }

        private void OnDisable()
        {
            if (beam != null)
                beam.enabled = false;
        }

        private void OnDestroy()
        {
            if (beamSprite != null)
                Destroy(beamSprite);
        }
    }
}
