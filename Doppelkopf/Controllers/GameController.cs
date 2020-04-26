using System;
using System.Collections.Generic;
using System.Linq;
using Doppelkopf.GameObjects;
using Doppelkopf.Interfaces;
using Doppelkopf.Models;
using Doppelkopf.Services;

namespace Doppelkopf.Controllers
{
    // controller per game
    public partial class GameController : IGameController
    {
        public int TableID { private set; get; }
        public string Name { private set; get; }
        public string Password { private set; get; }
        public bool Hidden { private set; get; }
        public List<User> Administrators { private set; get;} = new List<User>();
        private readonly List<User> users;
        private readonly List<Player> players = new List<Player>();
        private readonly List<IClientConnectionController> clientControllers;
        private readonly ISendService sendService;
        private readonly PlayerToActQueue playerToActQueue;
        private readonly RuleSet ruleSet;
        private List<Player> currentPlayersToAct = new List<Player>();
        private int currentPlayerToActID;
        private readonly List<Card> allCards = new List<Card>();
        private readonly Dictionary<Player, List<Card>> assignedHands = new Dictionary<Player, List<Card>>();
        private readonly Dictionary<Player, HandCharacteristics> handCharacteristics = new Dictionary<Player, HandCharacteristics>();
        private readonly Dictionary<Player, GameType> gameAnnouncements = new Dictionary<Player, GameType>();
        private GameType currentGameType;

        public enum State
        {
            Pause,
            Shuffling,
            Premove,
            Move,
            CollectTrick,
            FinishRound
        }

        private State state;

        public GameController(ISendService sendService, List<User> users, List<IClientConnectionController> clientControllers, int tableID, string name, string password, bool hidden, User foundingUser, RuleSet ruleSet)
        {
            this.sendService = sendService;
            this.users = users;
            this.clientControllers = clientControllers;
            TableID = tableID;
            Name = name;
            Password = password;
            Hidden = hidden;
            state = State.Pause;
            Administrators.Add(foundingUser);
            playerToActQueue = new PlayerToActQueue(players);
            this.ruleSet = ruleSet;

            initializeAllCards();
        }

        private void initializeAllCards()
        {
            for (int n = 0; n < 2; ++n) // DOPPELkopf :)
            {
                foreach (var value in (Card.Value[])Enum.GetValues(typeof(Card.Value)))
                {
                    if (value == Card.Value.NINE && !ruleSet.UseNines)
                    {
                        continue;
                    }

                    foreach (var suit in (Card.Suit[])Enum.GetValues(typeof(Card.Suit)))
                    {
                        allCards.Add(new Card(value, suit));
                    }
                }
            }
        }

        private void updatePlayers()
        {
            // add users as players that are on this table if they do not exist yet
            var nonPlayerUsers = users.Where(user => user.TableID == TableID && !players.Exists(player => player.User == user));

            foreach (var nonPlayerUser in nonPlayerUsers)
            {
                players.Add(new Player { User = nonPlayerUser, SittingOut = false });
            }

            // set players as sitting out if they have not this table ID
            var nonUserPlayers = players.Where(player => player.User.TableID != TableID || !player.User.Online);

            foreach (var nonUserPlayer in nonUserPlayers)
            {
                nonUserPlayer.SittingOut = true;
            }
        }

        public State HandleMessage(ClientMessage message)
        {
            // just update all the time, should be cheap enough
            updatePlayers();

            switch(state)
            {
                case State.Pause:
                    return handlePauseState(message);
                case State.Shuffling:
                    return handleShufflingState(message);
                case State.Premove:
                    return handlePreMoveState(message);
                case State.Move:
                    // return handleMoveState(message);
                case State.FinishRound:
                    // return handleFinishRoundState(message);
                case State.CollectTrick:
                    // return handleCollectTrickState(message);
                    break;
            }

            return state;
        }
    }
}
