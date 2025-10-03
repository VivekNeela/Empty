using UnityEngine;
using TMPro;
using Unity.Netcode;

namespace tmkoc.physicsAdv
{

    public enum States
    {
        Accelarate,
        Deccelarate,
        Brake,
        TurnOff
    }

    public enum Mode
    {
        Torque,
        Force
    }



    public class CarController : NetworkBehaviour
    {
        [SerializeField] Mode moveMode;
        [SerializeField] States currectState;
        [SerializeField] Rigidbody2D frontWheel;
        [SerializeField] Rigidbody2D backWheel;
        [SerializeField] Rigidbody2D carBody;
        [SerializeField] bool followCarOnAcceleration = true;
        [SerializeField] ParticleSystem backWheelDust;
        [SerializeField] Vector2 particleYOffset;
        public float acceleration;
        public bool isGrounded;
        [SerializeField] Transform groundCheck;
        public float maxSpeed = 1000;
        bool carStarted = false;
        public TextMeshProUGUI speedText;
        public AudioClip carAud, brake;



        //PhysicsCarAudioManager m_audio;
        void Start()
        {
            // m_audio = AudioManager.Instance as PhysicsCarAudioManager;
            // m_audio.sfxSource.Stop();
            //currectState = States.Brake;
            //carBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
            frontWheel.sleepMode = RigidbodySleepMode2D.StartAsleep;
            backWheel.sleepMode = RigidbodySleepMode2D.StartAsleep;

            //UIManager.Instance.BrakeButton.gameObject.SetActive(false);

            // Camera.main.GetComponent<CameraController>().SetCarCamFollow(transform);
            // Camera.main.GetComponent<CameraController>().SwitchToOverviewCam();


        }


        void OnDisable()
        {
            Physics2D.gravity = new Vector3(0, -9.8f, 0);
        }

        private void Update()
        {

            if (!IsOwner) return;

            if (Input.GetKey(KeyCode.D))
                Accelerate();
            else if (Input.GetKey(KeyCode.A))
                Deccelerate();
            else
                Brake();


        }

        private void FixedUpdate()
        {
            switch (currectState)
            {
                case States.Brake:

                    frontWheel.angularVelocity = 0;
                    backWheel.angularVelocity = 0;
                    break;

                case States.Accelarate:
                    if (frontWheel.angularVelocity > -maxSpeed || backWheel.angularVelocity > -maxSpeed)
                    {
                        // frontWheel.AddTorque(-acceleration * 2 * Time.fixedDeltaTime);
                        backWheel.AddTorque(-acceleration * 2 * Time.fixedDeltaTime);
                        //carBody.AddTorque(speed * Time.fixedDeltaTime);
                    }
                    break;

                case States.Deccelarate:
                    if (frontWheel.angularVelocity < -maxSpeed || backWheel.angularVelocity < -maxSpeed)
                    {
                    }
                    // frontWheel.AddTorque(acceleration * 2 * Time.fixedDeltaTime);
                    backWheel.AddTorque(acceleration * 2 * Time.fixedDeltaTime);
                    //carBody.AddTorque(speed * Time.fixedDeltaTime);
                    break;

                case States.TurnOff:
                    break;
            }
        }



        public void CheckGameOver()
        {
            // if (GameManager.Instance.currentState != GameState.LevelCompleted)
            // {
            //     GameManager.Instance.RestartGame();
            // }
        }

        public void SetState(States state)
        {
            currectState = state;
            //Debug.Log("Stated changed to "+state);
        }

        public void Accelerate()
        {
            // m_audio.PlayAudio(m_audio.sfxSource, m_audio.engineAudio, true);
            // m_audio.PlayAudio(m_audio.sfxSource, m_audio.accelAudio, true, loop: true);
            carStarted = true;
            currectState = States.Accelarate;
            // AudioManager.Instance.PlayAudio(AudioManager.Instance.sfxSource, carAud, true);


            //if (followCarOnAcceleration) Camera.main.GetComponent<CameraController>().SwitchToCarCam();
        }

        public void Deccelerate()
        {
            // m_audio.PlayAudio(m_audio.sfxSource, m_audio.engineAudio, true);
            // m_audio.PlayAudio(m_audio.sfxSource, m_audio.accelAudio, true, loop: true);
            carStarted = true;
            currectState = States.Deccelarate;
            // AudioManager.Instance.PlayAudio(AudioManager.Instance.sfxSource, carAud, true);


            //if (followCarOnAcceleration) Camera.main.GetComponent<CameraController>().SwitchToCarCam();
        }

        public void Brake()
        {
            // m_audio.sfxSource.Stop();
            // m_audio.PlayAudio(m_audio.sfxSource, m_audio.brakeAudio, true);
            currectState = States.Brake;
            // AudioManager.Instance.PlayAudio(AudioManager.Instance.sfxSource, brake, true);
        }

        public void TurnOff()
        {
            currectState = States.TurnOff;
        }



        public void UpdateSpeed(float value)
        {
            if (value == 0) value = maxSpeed;
            maxSpeed = value;
            speedText.text = (maxSpeed / 50).ToString("F1") + " km/h";
        }

        public void UpdateMass(float value, TextMeshProUGUI text)
        {
            if (value == 0) value = carBody.mass;
            carBody.mass = value;
            text.text = (value * 1000).ToString("F1") + " kg";
        }

        public void UpdateAcceleration(float value, TextMeshProUGUI text)
        {
            if (value == 0) value = acceleration;
            acceleration = value;
            text.text = (acceleration / 50).ToString("F1") + " km/h²";
        }

        void OnDrawGizmos()
        {
            if (isGrounded) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;
            Gizmos.DrawSphere(backWheel.position - particleYOffset, 0.2f);
        }

    }
}