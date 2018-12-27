using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advent_24_1
{
    class Group
    {
        public int Size { get; set; }
        public int AttackDamage { get; set; }
        public string AttackType { get; set; }
        public int HP { get; set; }
        public int Initiative { get; set; }

        public List<string> Weaknesses { get; set; }
        public List<string> Immunities { get; set; }

        public Group()
        {
            Weaknesses = new List<string>();
            Immunities = new List<string>();
        }

        public int EffectivePower()
        {
            return Size * AttackDamage;
        }

        public int DamageTaken(int effectivePower, string attackType)
        {
            int multiplier = 1;
            if (Immunities.Contains(attackType))
            {
                return 0;
            }
            else if (Weaknesses.Contains(attackType))
            {
                multiplier = 2;
            }

            return multiplier * effectivePower;
        }

        public void TakeDamage(int effectivePower, string attackType)
        {
            var damage = DamageTaken(effectivePower, attackType);
            Size -= damage / HP;
            if (Size < 0)
            {
                Size = 0;
            }
        }

    }
}
