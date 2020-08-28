using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script is modified, based on claygamestudio.com﻿ http://gamedevmalang.com/dynamic-2d-water-in-unity/
//See thanks.txt for credits

namespace ilhamhe {

	public class DynamicWater2D : MonoBehaviour {

        public Transform top, bottom, left, right;

		//[System.Serializable]
		public struct Bound {
			public float top;
			public float right;
			public float bottom;
			public float left;
		}

        [Header("Water Settings")]
        public bool noInitialSplash = true;  //If objects start static in the water, then when the scene loads the script will detect it as something that dropped in and create a splash.
                                             //If we set noInitialSplash to true, it will prevent splashes for the first few frames of the scene
        private bool canSplash = true;       //Set to false to prevent splashes. Used for noInitialSplash, can be used to turn splashing off. noInitialSplash being set to true will make this change to true after the first couple frames
        public float splashVelocityThreshold = 2.5f;  //Object's y velocity must be below -splashVelocityThreshold or above +splashVelocityThreshold value to create a splash

        public string sortingLayerName;
		private Bound bound;
		public int quality;

		public Material waterMaterial;
		public GameObject splash;
        public GameObject bubbles;
        private GameObject bubblesGo;

		private Vector3[] vertices;

		private Mesh mesh;

        [Header("Sound")]
        public List<AudioClip> sndSplash;
        public float randomizeSplashPitchMin = 0.8f, randomizeSplashPitchMax = 1.2f;
        public bool onlyPlayIfSurfaceIsOnScreen = true;
        public float minimumForceForSound = 4f;

        [Header("Physics Settings")]
        public bool simulateSurface = true;
		public float springconstant = 0.02f;
		public float damping = 0.1f;
		public float spread = 0.1f;
		public float collisionVelocityFactor = 0.04f;
        public float sampleRate = 25f; //The number of physics updates to do per second

		float[] velocities;
		float[] accelerations;
		float[] leftDeltas;
		float[] rightDeltas;

		private float timer;

        //We can use two buoyancy effectors to gain more control over which objects sink and which ones float. High buoyancy (floaty) and low (sinky)
        private BuoyancyEffector2D bEffectorOne, bEffectorTwo;
        public GameObject buoyancyEffectorTwoGo;  //Since we can only have one buoyancy effector per object, we'll use a separate child object to control low buoyancy

        //private BoxCollider2D surfaceOnlyTrigger;

        Global global;
        cameraFollowPlayer cameraFollow;

        private void Start () {
            global = GameObject.FindWithTag("global").GetComponent<Global>();
            if (!cameraFollow)
            {
                cameraFollow = Camera.main.GetComponent<cameraFollowPlayer>();
            }

            bEffectorOne = GetComponent<BuoyancyEffector2D>() as BuoyancyEffector2D;
            if (buoyancyEffectorTwoGo!=null) bEffectorTwo = buoyancyEffectorTwoGo.GetComponent<BuoyancyEffector2D>() as BuoyancyEffector2D;

            if (noInitialSplash)
            {
                canSplash = false;
                Invoke("initialSplashSwitch", 0.1f);
            }
            Reset();

            InvokeRepeating("UpdateSurface", 0.0f, 1.0f / sampleRate);
        }
        private void initialSplashSwitch()
        {
            canSplash = true;
        }

        //This function resets / initializes the water. Can be called at start, or if properties such as water bounds have changed.
        public void Reset()
        {
            ResetBounds();
            InitializePhysics();
            GenerateMesh();
            SetBoxCollider2D();
            SetupBubbles();
        }

        //This script resets the bounding positions of the water
        public void ResetBounds()
        {
            bound.top = top.position.y;
            bound.bottom = bottom.position.y;
            bound.left = left.position.x;
            bound.right = right.position.x;
        }

        public void SetupBubbles()
        {
            if (bubbles == null) return;
            if (bubblesGo != null) Destroy(bubblesGo);

            bubblesGo = Instantiate(bubbles, gameObject.transform);
            bubblesGo.transform.position = new Vector3(bound.left + (delta(bound.right, bound.left) / 2), bound.bottom + (delta(bound.top, bound.bottom) / 2),0);
            ParticleSystem bubblesPs = bubblesGo.GetComponent<ParticleSystem>();
            var sh = bubblesPs.shape;

            sh.scale = new Vector3(delta(bound.right,bound.left), delta(bound.top,bound.bottom), 0);

            var collider = GameObject.CreatePrimitive(PrimitiveType.Plane);
            collider.transform.parent = bubblesPs.transform;
            collider.transform.localPosition = new Vector3(0, delta(bound.top, bound.bottom) / 2, 0);
            collider.transform.localScale = new Vector3(delta(bound.right, bound.left), delta(bound.top, bound.bottom), 0);
            collider.transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));

            var collision = bubblesPs.collision;
            collision.SetPlane(0, collider.transform);
        }

        public float delta(float a, float b)
        {
            if (a > b) return a - b;
            else return b - a;
        }

		private void InitializePhysics () {
			velocities = new float[quality];
			accelerations = new float[quality];
			leftDeltas = new float[quality];
			rightDeltas = new float[quality];
		}

		private void GenerateMesh () {
			float range = (bound.right - bound.left) / (quality - 1);
			vertices = new Vector3[quality * 2];

			// generate vertices
			// top vertices
			for (int i = 0; i < quality; i++) {
				vertices[i] = new Vector3 (bound.left + (i * range), bound.top, 0);
			}
			// bottom vertices
			for (int i = 0; i < quality; i++) {
				vertices[i + quality] = new Vector2 (bound.left + (i * range), bound.bottom);
			}

			// generate tris. the algorithm is messed up but works. lol.
			int[] template = new int[6];
			template[0] = quality;
			template[1] = 0;
			template[2] = quality + 1;
			template[3] = 0;
			template[4] = 1;
			template[5] = quality + 1;

			int marker = 0;
			int[] tris = new int[((quality - 1) * 2) * 3];
			for (int i = 0; i < tris.Length; i++) {
				tris[i] = template[marker++]++;
				if (marker >= 6) marker = 0;
			}

			// generate mesh
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer> ();

            if (sortingLayerName != "") meshRenderer.sortingLayerName = sortingLayerName;

            if (waterMaterial) meshRenderer.sharedMaterial = waterMaterial;

			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter> ();

			mesh = new Mesh ();
			mesh.vertices = vertices;
			mesh.triangles = tris;
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();

			// set up mesh
			meshFilter.mesh = mesh;
		}

		private void SetBoxCollider2D () {
			BoxCollider2D col = gameObject.AddComponent<BoxCollider2D> ();
			col.isTrigger = true;

            if (bEffectorOne != null)
            {
                //Create a separate collider for buoyancy so we have more control over what sinks and what floats
                BoxCollider2D ecol = gameObject.AddComponent<BoxCollider2D>();
                ecol.isTrigger = true;
                ecol.usedByEffector = true;
            }
            if (bEffectorTwo != null)
            {
                //Let's add a collider to our second buorancy effector object
                buoyancyEffectorTwoGo.transform.position = new Vector3(bound.left + (delta(bound.right, bound.left) / 2), bound.bottom + (delta(bound.top, bound.bottom) / 2), 0);
                BoxCollider2D scol = buoyancyEffectorTwoGo.AddComponent<BoxCollider2D>();
                scol.isTrigger = true;
                scol.usedByEffector = true;
                scol.size = new Vector2(delta(bound.right, bound.left), delta(bound.top, bound.bottom));
                bEffectorTwo.surfaceLevel += (delta(bound.top, bound.bottom) / 2);
            }

         /* surfaceOnlyTrigger = gameObject.AddComponent<BoxCollider2D>();
            surfaceOnlyTrigger.isTrigger = true;
            surfaceOnlyTrigger.size = new Vector2(surfaceOnlyTrigger.size.x, 0.1f);
            surfaceOnlyTrigger.offset += new Vector2(0, delta(bound.top,bound.bottom)/2);*/
        }

		private void UpdateSurface () {
            if (simulateSurface)
            {
                // optimization. we don't want to calculate all of this on every call.
                if (timer <= 0) return;
                timer -= Time.deltaTime;

                // updating physics
                for (int i = 0; i < quality; i++)
                {
                    float force = springconstant * (vertices[i].y - bound.top) + velocities[i] * damping;
                    accelerations[i] = -force;
                    vertices[i].y += velocities[i];
                    velocities[i] += accelerations[i];
                }

                for (int i = 0; i < quality; i++)
                {
                    if (i > 0)
                    {
                        leftDeltas[i] = spread * (vertices[i].y - vertices[i - 1].y);
                        velocities[i - 1] += leftDeltas[i];
                    }
                    if (i < quality - 1)
                    {
                        rightDeltas[i] = spread * (vertices[i].y - vertices[i + 1].y);
                        velocities[i + 1] += rightDeltas[i];
                    }
                }

                // updating mesh
                mesh.vertices = vertices;
            }
		}

		private void OnTriggerEnter2D(Collider2D col) {
			Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
			if (rb!=null) Splash(col, rb.velocity.y);
		}

		public void Splash (Collider2D col, float force) {

            if (!canSplash) return;

            //Don't splash unless we've hit hard enough
            if (force > -splashVelocityThreshold && force < splashVelocityThreshold) return;

            float iforce = force;

            force *= collisionVelocityFactor;
            timer = 3f;
			float radius = col.bounds.max.x - col.bounds.min.x;
			Vector2 center = new Vector2(col.bounds.center.x, bound.top) ;
			// instantiate splash particle
			GameObject splashGO = Instantiate(splash, new Vector3(center.x, center.y, 0), Quaternion.Euler(0,0,60));
			Destroy(splashGO, 2f);

			// applying physics
			for (int i = 0; i < quality; i++) {
				if (PointInsideCircle (vertices[i], center, radius)) {
					
					velocities[i] = force;
				}
			}

            //Sounds
            if (sndSplash.Count > 0 && (iforce < -minimumForceForSound || iforce > minimumForceForSound) )
            {
                bool play = true;
                if (onlyPlayIfSurfaceIsOnScreen) play = isWaterSurfaceOnScreen();
                if (play) global.audio.RandomSoundEffect(sndSplash.ToArray(), randomizeSplashPitchMin, randomizeSplashPitchMax);
            }
        }

        public bool isWaterSurfaceOnScreen()
        {
            if (!cameraFollow)
                cameraFollow = Camera.main.GetComponent<cameraFollowPlayer>();
            if (!cameraFollow) return true;
            float cl = cameraFollow.getLeftViewX() - 1f;
            float cr = cameraFollow.getRightViewX()+1f;
            float ct = cameraFollow.getTopViewY()+1f;
            float cb = cameraFollow.getBottomViewY()-1f;

            Rect cam = new Rect(cl, cb, cr - cl, ct - cb);

            if (cam.Overlaps(new Rect(bound.left, bound.top, bound.right, bound.top + 1f)))
                return true;
            else
                return false;
        }

		bool PointInsideCircle (Vector2 point, Vector2 center, float radius) {
			return Vector2.Distance (point, center) < radius;
		}

    }

}