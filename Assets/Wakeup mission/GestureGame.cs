using ReadingMission;
using UnityEngine;
using UnityEngine.UI;

namespace Wakeup_mission {
    public class GestureGame : MonoBehaviour {
        public static int gesture = -1; //1=yes, 2=no
        public static bool new_gesture = false;
        public static bool mission_ended = false;
        public static bool mission_won = false;

        public RawImage i1, i2, i3, i4, i5;
        public Text timertext;
        public Texture updown, leftright;

        private int current_sym;
        private int current_big;
        private int nextans;
        private int a1, a2, a3, a4, a5;
        private bool game_in_progress = false;

        private float timelimit = float.MaxValue;
        private float time_left;
        private ReadingLoop _mainGame = null;

        // Use this for initialization
        void Start () {
            TurnOff();
        }
	
        // Update is called once per frame
        void Update () {
            if (game_in_progress)
            {
                time_left -= Time.deltaTime;
                if (time_left < 0.0f) 
                { 
                    lose();
                    return;
                }
                update_timetext();

                symbig_match();
                if (new_gesture)
                {
                    handle_newges();
                    new_gesture = false;

                    if (current_sym == 6)
                    {
                        win();
                    }
                    calc_next_ans();
                }
            }
        }

        int randomize(RawImage input)
        {
            float dice = Random.Range(-1.0f, 1.0f);
            if (dice >= 0.0f){
                input.texture = updown;
                return 1;
            }
            else
            {
                input.texture = leftright;
                return 2;
            }
        }

        void reset_game()
        {
            game_in_progress = true;
//            i1.enabled = true;
//            i2.enabled = true;
//            i3.enabled = true;
//            i4.enabled = true;
//            i5.enabled = true;
            timertext.enabled = true;

            a1 = randomize(i1);
            a2 = randomize(i2);
            a3 = randomize(i3);
            a4 = randomize(i4);
            a5 = randomize(i5);

            time_left = timelimit;
            current_sym = 1;
            current_big = 5;
        }
    
        void symbig_match()
        {
            if (current_sym == current_big) { return; }
            // change occured
            i1.enabled = false;
            i2.enabled = false;
            i3.enabled = false;
            i4.enabled = false;
            i5.enabled = false;

            if (current_sym <= 5) {
                i5.enabled = true;
            }
            if (current_sym <= 4) {
                i4.enabled = true;
            }
            if (current_sym <= 3) {
                i3.enabled = true;
            }
            if (current_sym <= 2) {
                i2.enabled = true;
            }
            if (current_sym <= 1) {
                i1.enabled = true;
            }

            current_big = current_sym;
        }
    
        void calc_next_ans()
        {
            if (current_sym == 1)
            {
                nextans = a1;
            }
            else if (current_sym == 2)
            {
                nextans = a2;
            }
            else if (current_sym == 3)
            {
                nextans = a3;
            }
            else if (current_sym == 4)
            {
                nextans = a4;
            }
            else if (current_sym == 5)
            {
                nextans = a5;
            }
            else
            {
                //shouln't get here
            }
        }

        void handle_newges()
        {
            if (gesture == nextans)
            {
                current_sym += 1;
            }
            new_gesture = false;
        }

        void update_timetext()
        {
            timertext.text = string.Format("{0:0.00}", time_left);
        }

        void win()
        {
            mission_ended = true;
            mission_won = true;
            TurnOff();
            game_in_progress = false;
            time_left = timelimit;
            _mainGame.OnWakeUp();
            _mainGame = null;
            print("game done and you won");
        }

        void lose()
        {
            mission_ended = true;
            TurnOff();
            game_in_progress = false;
            time_left = timelimit;
            print("game done and you lose");
        }

        public void TurnOff()
        {
            i1.enabled = false;
            i2.enabled = false;
            i3.enabled = false;
            i4.enabled = false;
            i5.enabled = false;
            timertext.enabled = false;
        }

        public void StartGame(ReadingLoop mainGame) {
            _mainGame = mainGame;
            reset_game();
            calc_next_ans();
        }
    }
}
