using Newtonsoft.Json;
using OverParse_NT.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverParse_NT.PSRT
{
    public struct PSRTMessage
    {
        [JsonRequired]
        [JsonProperty("identifier")]
        public string Identifier;

        [JsonRequired]
        [JsonProperty("message")]
        public object Message;
    }

    public struct DamageMessage
    {
        public enum DamageFlags : byte
        {
            JustAttack = 1,
            Misc = 4,
            Damage = 8,
            MultiHit = 16,
            Misc2 = 32,
            Critical = 64
        }

        [JsonProperty("account")]
        public uint Account;

        [JsonProperty("target")]
        public uint Target;

        [JsonProperty("instance")]
        public uint Instance;

        [JsonProperty("source")]
        public uint Source;

        [JsonProperty("action")]
        public uint Action;

        [JsonProperty("value")]
        public int Value;

        [JsonProperty("flags")]
        public DamageFlags Flags;
    }

    public struct CharacterInfoMessage
    {
        [JsonProperty("account")]
        public uint Account;

        [JsonProperty("name")]
        public string Name;
    }

    public class PSRTGenerator : IGenerator
    {
        public event EventHandler<GeneratorEncounterDataChangedEventArgs> EncounterDataChanged;

        private PartialEncounter _CurrentEncounter;
        private ClientWebSocket _Socket = new ClientWebSocket();

        private Dictionary<uint, EncounterPlayer> _CharacterCache = new Dictionary<uint, EncounterPlayer>();

        public async Task RunAsync(CancellationToken ct)
        {
            await _Socket.ConnectAsync(new Uri("ws://localhost:48480"), ct);

            var messageBuffer = new List<byte>();
            var buffer = new ArraySegment<byte>(new byte[2048]);

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var receive = await _Socket.ReceiveAsync(buffer, ct);

                messageBuffer.AddRange(buffer.Take(receive.Count));

                if (!receive.EndOfMessage)
                    continue;

                var messageString = Encoding.UTF8.GetString(messageBuffer.ToArray());
                messageBuffer.Clear();

                var message = JsonConvert.DeserializeObject<PSRTMessage>(messageString);

                switch (message.Identifier)
                {
                    case "pso2.character_info":
                        {
                            var characterInfoMessage = JsonConvert.DeserializeObject<CharacterInfoMessage>(message.Message.ToString());

                            if (!_CharacterCache.ContainsKey(characterInfoMessage.Account))
                            {
                                _CharacterCache[characterInfoMessage.Account] = new EncounterPlayer
                                {
                                    Account = characterInfoMessage.Account,
                                    Name = characterInfoMessage.Name
                                };
                            }
                            else
                            {
                                _CharacterCache[characterInfoMessage.Account].Name = characterInfoMessage.Name;
                            }
                        }
                        break;
                    case "pso2.damage":
                        {
                            var damageMessage = JsonConvert.DeserializeObject<DamageMessage>(message.Message.ToString());

                            if (_CurrentEncounter == null)
                                _CurrentEncounter = new PartialEncounter
                                {
                                    Start = DateTime.UtcNow,
                                    Steps = new List<EncounterStep>()
                                };

                            // create new encounter instance to avoid race conditions
                            var encounterSteps = new List<EncounterStep>(_CurrentEncounter.Steps);
                            _CurrentEncounter = new PartialEncounter
                            {
                                Start = _CurrentEncounter.Start,
                                Steps = encounterSteps
                            };

                            if (!_CharacterCache.ContainsKey(damageMessage.Account))
                                _CharacterCache[damageMessage.Account] = new EncounterPlayer { Account = damageMessage.Account, Name = damageMessage.Account.ToString() };

                            encounterSteps.Add(new EncounterStep
                            {
                                Offset = DateTime.UtcNow - _CurrentEncounter.Start,

                                Source = _CharacterCache[damageMessage.Account],

                                Ability = new EncounterAbility
                                {
                                    Account = damageMessage.Action,
                                    Name = "Todo",

                                    AbilityType = damageMessage.Value < 0 ? EncounterAbilityType.Heal : EncounterAbilityType.Damage,
                                    Value = Math.Abs(damageMessage.Value),

                                    IsJustAttack = (damageMessage.Flags & DamageMessage.DamageFlags.JustAttack) != 0,
                                    IsCriticalAttack = (damageMessage.Flags & DamageMessage.DamageFlags.Critical) != 0,
                                }
                            });

                            EncounterDataChanged?.Invoke(this, new GeneratorEncounterDataChangedEventArgs { Current = _CurrentEncounter });
                        }
                        break;
                    default:
                        return;
                }
            }
        }
    }
}
