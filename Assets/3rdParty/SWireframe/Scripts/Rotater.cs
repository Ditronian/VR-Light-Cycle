
using UnityEngine;

namespace S.Wireframe
{

    public class Rotater : MonoBehaviour
    {
        public bool move = false;
        public float speed = 0.2f;
        private Transform trans;
        private Vector3 srcPos;

        private void Awake()
        {
            this.trans = transform;
            this.srcPos = this.trans.position;
        }

        void Update()
        {
            this.trans.localEulerAngles += new Vector3(0.0f, speed, 0.0f);
            if( move )
                this.trans.position = new Vector3(this.srcPos.x, this.srcPos.y + 0.5f * Mathf.Abs(Mathf.Sin(Time.time)), this.srcPos.z);
        }
    }
}
