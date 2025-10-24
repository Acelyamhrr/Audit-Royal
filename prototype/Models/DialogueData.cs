using System;
using System.Collections.Generic;

namespace prototype.Models {
    public class DialogueData {

        public Dictionary<string, string[]> Dialogues {
            get;
            set;

        } = new Dictionary<string, string[]>();

        /* la clé (string) : id du dialogue
            la valeur (string[]) : tab de phrases possible pour la clé
        */
    }
}