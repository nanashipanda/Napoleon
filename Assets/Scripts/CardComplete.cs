using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CardComplete : MonoBehaviour
{
    public GameObject[] Cards;
    public GameObject GameController;
    public Field field;
    //その回の親と、誰のターンかの変数
    public int parent = 0;
    public int turn = 0;

    GameObject Spade;
    GameObject Heart;
    GameObject Clover;
    GameObject Diamond;

    public static int battle = 0;

    int i, j;

    //シャッフル用の配列
    int[] card = new int[54];

    //プレイヤー5人と余りの作成
    public GameObject[] Player = new GameObject[5];
    Player[] player = new Player[5];
    List<GameObject> rest = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        //ボタンの非表示
        Spade = GameObject.Find("Canvas/Spade");
        Heart = GameObject.Find("Canvas/Heart");
        Clover = GameObject.Find("Canvas/Clover");
        Diamond = GameObject.Find("Canvas/Diamond");

        Spade.SetActive(false);
        Heart.SetActive(false);
        Clover.SetActive(false);
        Diamond.SetActive(false);

        //コンストラクタ的な
        for (i = 0; i < 5; i ++)
        {
            Player[i] = GameObject.Find("player" + i);
        }
        for (i = 0; i < 5; i++)
        {
            player[i] = new Player();
            player[i].hand = new List<GameObject>();
        }

        field = new Field();
        //ここまで

        //シャッフルする
        for (i = 0; i < 54; i++) card[i] = i;
        card = shuffle(card);

        //プレイヤー5人に手札を配る
        for (i = 0; i < 5; i++)
        {
            for (j = 0; j < 10; j++)
            {
                //カードを上から10枚ずつくばってプレイヤーのナンバー情報をつける
                Cards[card[i * 10 + j]].GetComponent<Deck>().p_num = i;
                Player[i].GetComponent<Player>().Add_card(Cards[card[(i * 10) + j]]);
                player[i].Add_card(Cards[card[(i * 10) + j]]);
            }
            //1人ずつ配り終えたらソートする
            Player[i].GetComponent<Player>().hand.Sort(delegate (GameObject a, GameObject b) { return string.Compare(a.name, b.name); });
            player[i].hand.Sort(delegate (GameObject a, GameObject b) { return string.Compare(a.name, b.name); });
        }

        //配った手札をそれぞれインスタンス化する、スケールもここで変更
        for(i = 0; i < 5; i++)
        {
            for(j = 0; j < 10; j++) {
                //player[0]の手札と位置
                if (i == 0)
                {
                    /*/*//*/*//*/*//*/*///To DO//*/*/*///Playerとplayerの設定
                    Player[i].GetComponent<Player>().hand[j] = Instantiate(Player[i].GetComponent<Player>().hand[j], new Vector3(-7.2f + j * 1.6f, -4f + 1.5f * i, transform.localPosition.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y + 180, transform.rotation.z)) as GameObject;
                    Player[i].GetComponent<Player>().hand[j].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                    player[i].hand[j] = Instantiate(player[i].hand[j], new Vector3(-7.2f + j * 1.6f, -4f + 1.5f * i, transform.localPosition.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y + 180, transform.rotation.z)) as GameObject;
                    player[i].hand[j].transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                }
                //player[0]以外（敵）の手札と位置
                else
                {
                    Player[i].GetComponent<Player>().hand[j] = Instantiate(Player[i].GetComponent<Player>().hand[j], new Vector3(-7.5f + j * 0.8f, -3.5f + 1.5f * i, transform.localPosition.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y + 180, transform.rotation.z)) as GameObject;
                    Player[i].GetComponent<Player>().hand[j].transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                    player[i].hand[j] = Instantiate(player[i].hand[j], new Vector3(-7.5f + j * 0.8f, -3.5f + 1.5f * i, transform.localPosition.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y + 180, transform.rotation.z)) as GameObject;
                    player[i].hand[j].transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                }
            }
        }
        /*/*//*/*ToDo : 余りの設定//*///*/
        //残り4枚をあまりとして場に
        for (i = 50; i < 54; i++)
        {
            rest.Add(Cards[card[i]]);
        }
        //以下fieldの位置とスケール設定
        for (i = 0; i < 4; i++)
        {
            rest[i].transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            Instantiate(rest[i], new Vector3(-7.5f + i * 0.8f, 4f, transform.localPosition.z), Quaternion.Euler(transform.rotation.x, transform.rotation.y + 180, transform.rotation.z));
        }
    }


    // Update is called once per frame
    void Update()
    {
        //Update内でクリック監視してカードの移動
        if (Input.GetMouseButton(0))
        {
            GameObject obj = getClickObject();
            if (obj != null)
            {
                //手札を出せる条件は上から
                //①クリックしたカードがその回の親のスートと同じ
                //②自分が親
                //③Joker
                //④その回の親のスートの手札がない
                //のどれか
                if (obj.GetComponent<Deck>().suit == field.suit
                    || parent == turn
                    || obj.GetComponent<Deck>().suit == 0
                    || !(player[turn].hand.Exists(delegate (GameObject c) { return c.GetComponent<Deck>().suit == field.suit; }))) 
                {
                    Round(obj.GetComponent<Deck>().p_num, obj);
                }
            }
        }
    }

    //1周分の処理
    void Round(int i, GameObject obj)
    {
        if (turn == i)
        {
            if (parent == i)
            {
                if (obj.GetComponent<Deck>().suit == 0)
                {
                    SetActiveButton();
                }
                else
                {
                    field.FieldSuit(obj.GetComponent<Deck>().suit);
                }
            }

            field.Layout(obj);
            //player0だけ別処理
            if (i == 0)
            {
                iTween.ScaleTo(obj, iTween.Hash("x", 0.4f, "y", 0.4f));  //0だけ
                iTween.MoveTo(obj, new Vector3(5.0f, -2.0f, 0f), 1.0f);
            }
            //player0以外
            else
            {
                iTween.MoveTo(obj, new Vector3(3.0f, -2.0f + 1.5f * (i - 1), 0f), 1.0f);
            }
            int index = player[i].MoveCard(obj.GetComponent<Deck>().id);
            Vector3[] pos = new Vector3[10];
            pos[index] = (player[i].hand.Find(x => x.Equals(obj)).transform.position);
            for (j = index + 1; j < 10 - battle; j++)
            {
                pos[j] = player[i].hand[j].transform.position;
                if (i == 0) pos[j].x -= 1.6f;
                else if(i > 0 && i < 5) pos[j].x -= 0.8f;
                player[i].hand[j].transform.position = pos[j];
            }

            player[i].hand.RemoveAt(index);

            if (parent == (i + 1) % 5)
            {
                parent = field.Judge();
                turn = parent;
                //field.clear();
                battle++;
            }
            else turn = (i + 1) % 5;

        }
    }


    //シャッフルメソッド
    int[] shuffle(int[] card)
    {
        int i = 0;
        for (i = 0; i < 54; i++)
        {
            int value = Random.Range(0, 54);
            int tmp = card[i];
            card[i] = card[value];
            card[value] = tmp;
        }

        return card;
    }

    //クリックしたオブジェクトの情報を引っ張ってくるメソッド
    private GameObject getClickObject()
    {
        GameObject result = null;
        //クリックされた場所のオブジェクト取得
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 tapPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D collition2d = Physics2D.OverlapPoint(tapPoint);
            if (collition2d)
            {
                result = collition2d.transform.gameObject;
            }
        }

        return result;
    }

    public void SetActiveButton()
    {
        Spade.SetActive(true);
        Heart.SetActive(true);
        Clover.SetActive(true);
        Diamond.SetActive(true);
    }

    public void SetDisActiveButton()
    {
        Spade.SetActive(false);
        Heart.SetActive(false);
        Clover.SetActive(false);
        Diamond.SetActive(false);
    }

    public void SetSpade()
    {
        field.suit = 1;
    }

    public void SetHeart()
    {
        field.suit = 2;
    }

    public void SetClover()
    {
        field.suit = 3;
    }

    public void SetDiamond()
    {
        field.suit = 4;
    }
}
