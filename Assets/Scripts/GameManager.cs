using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private List<Card> deck;
    private List<GameObject> players;
    private List<HashSet<int>> playerScores;
    HashSet<int> dealerScore;
    
    List<GameObject> everyCard = new List<GameObject>();

    public Transform table;
    public GameObject dealerObject;
    public GameObject cardObject;
    public GameObject playerObject;

    int currentOffset = 0;
    int currentPlayer = 0;

    [Header("Game Settings")]
    public int numberOfPlayer = 1;


    // Start is called before the first frame update
    void Start()
    {
        dealerScore = new HashSet<int>();
        dealerScore.Add(0);
        SetupDeck();
        SetupPlayers(numberOfPlayer);
        
        SetupGame();
    }

    void SetupDeck()
    {
        
        deck = new List<Card>();
        for (int k = 0; k < 6; k++)
        {
            for (int i = 2; i < 11; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    deck.Add(new Card(i, i.ToString()));
                }
            }
            for (int j = 0; j < 4; j++)
            {
                deck.Add(new Card(10, "K"));
                deck.Add(new Card(10, "Q"));
                deck.Add(new Card(10, "J"));
                deck.Add(new Card(0, "A"));
            }
        }

        Shuffle(deck);
    }

    void Shuffle(List<Card> orderedDeck)
    {
        deck = orderedDeck.OrderBy(x => Random.value).ToList();
    }

    void SetupPlayers(int n)
    {
        players = new List<GameObject>();
        playerScores = new List<HashSet<int>>();
        for(int i = 0; i < n; i++)
        {
            GameObject player = Instantiate(playerObject, table);
            players.Add(player);
            HashSet<int> score = new HashSet<int>();
            score.Add(0);
            playerScores.Add(score);
        }
        players[0].transform.GetChild(0).gameObject.SetActive(true);
    }

    void SetupGame()
    {
        DrawSelf();
        
        for (int i = 0; i < numberOfPlayer; i++)
        {
            Draw(i);
            Draw(i);
            currentOffset = 0;
        }

        currentOffset = 6;
    }
    
    void Draw(int i, bool doubleDraw = false)
    {
        Card drawed = deck.FirstOrDefault();
        deck.Remove(drawed);

        GameObject spawnedCard = GameObject.Instantiate(cardObject, players[i].transform);
        everyCard.Add(spawnedCard);
        foreach (TMP_Text text in spawnedCard.GetComponentsInChildren<TMP_Text>())
        {
            text.text = drawed.cardName;
        }

        spawnedCard.transform.position = new Vector3(spawnedCard.transform.position.x + currentOffset,
            spawnedCard.transform.position.y + currentOffset, 0);
        
        if (doubleDraw)
        {
            spawnedCard.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        
        currentOffset += 3;
        
        HashSet<int> newScore = new HashSet<int>();
        
        if(drawed.cardValue == 0)
        {
            foreach (int score in playerScores[i])
            {
                if(score+1<=21) newScore.Add(score + 1);
                if(score+11<=21) newScore.Add(score + 11);
            }
        }
        else
        {
            foreach (int score in playerScores[i])
            {
                int t = score + drawed.cardValue;
                if (t <= 21) newScore.Add(score + drawed.cardValue);
            }
        }
        
        playerScores[i] = newScore;
        DisplayScore(i);
        if (playerScores[i].Count == 0)
        {
            NextPlayer();
        }
    }

    void DrawSelf()
    {
        Card drawed = deck.FirstOrDefault();
        deck.Remove(drawed);

        GameObject spawnedCard = GameObject.Instantiate(cardObject, dealerObject.transform);
        everyCard.Add(spawnedCard);
        foreach (TMP_Text text in spawnedCard.GetComponentsInChildren<TMP_Text>())
        {
            text.text = drawed.cardName;
        }

        spawnedCard.transform.position = new Vector3(spawnedCard.transform.position.x + currentOffset,
            spawnedCard.transform.position.y + currentOffset, 0);
        
        HashSet<int> newScore = new HashSet<int>();
        
        if(drawed.cardValue == 0)
        {
            foreach (int score in dealerScore)
            {
                if(score+1<=21) newScore.Add(score + 1);
                if(score+11<=21) newScore.Add(score + 11);
            }
        }
        else
        {
            foreach (int score in dealerScore)
            {
                int t = score + drawed.cardValue;
                if (t <= 21) newScore.Add(score + drawed.cardValue);
            }
        }
        
        dealerScore = newScore;
        
        if (dealerScore.Count == 0)
        {
            dealerObject.GetComponent<TMP_Text>().text = "BUST";
        }
        else
        {
            dealerObject.GetComponent<TMP_Text>().text = dealerScore.Max().ToString();
        }
    }

    public void Draw()
    {
        Draw(currentPlayer);
    }
    
    public void Stand()
    {
        NextPlayer();
    }
    
    public void Double()
    {
        int i = currentPlayer;
        currentOffset += 2;
        Draw(currentPlayer, true);
        if(i==currentPlayer) NextPlayer();
    }
    
    public void NextPlayer()
    {
        players[currentPlayer].transform.GetChild(0).gameObject.SetActive(false);
        currentPlayer++;
        if (currentPlayer >= numberOfPlayer)
        {
            DealerTurn();
            
        }
        else
        {
            players[currentPlayer].transform.GetChild(0).gameObject.SetActive(true);
            currentOffset = 6;
        }
    }
    
    void DealerTurn()
    {
        currentOffset = 3;
        dealerObject.transform.GetChild(0).gameObject.SetActive(true);
        while (dealerScore.Max() < 17)
        {
            DrawSelf();
            currentOffset += 3;
        }
    }
    
    void DisplayScore(int i)
    {
        TMP_Text scoreText = players[i].transform.GetComponent<TMP_Text>();
        scoreText.text = "";
        foreach (int score in playerScores[i])
        {
            scoreText.text += score + "\n";
        }
        if(scoreText.text == "")
        {
            scoreText.text = "BUST";
        }
    }

    public void Restart()
    {
        foreach (var card in everyCard)
        {
            Destroy(card);
        }
        everyCard.Clear();
        dealerScore.Clear();
        dealerScore.Add(0);
        currentPlayer = 0;
        currentOffset = 0;
        HashSet<int> empty = new HashSet<int>();
        empty.Add(0);
        for (int i = 0; i < numberOfPlayer; i++)
        {
            playerScores[i] = empty;
            DisplayScore(i);
        }
        
        SetupGame();
    }
}
