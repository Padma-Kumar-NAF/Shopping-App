import { Component, signal } from '@angular/core';
import { sign } from 'crypto';
import { toast } from 'ngx-sonner';

type Letter = {
  letter: string;
  color: string;
};

@Component({
  selector: 'app-game',
  templateUrl: './game.html',
  styleUrl: './game.css',
})
export class Game {
  count = signal(0);
  exactAnswer = signal('Trash');
  answer = signal('Hello');

  outputs = signal<Letter[][]>([]);

  onCheck() {
    if(this.count() == 5){
      toast.error("Limit reached")
      return;
    }
    this.count.update(c => c+1);
    if (this.answer().length !== 5) {
      toast.error('The value length should be 5');
      return;
    }

    const word = this.answer();
    const row: Letter[] = [];

    for (let i = 0; i < word.length; i++) {
      let temp = this.exactAnswer().toLowerCase();


      if (word[i] === temp[i]) {
        row.push({
          letter: word[i],
          color: 'green',
        });
      } else if (temp.includes(word[i])) {
        row.push({
          letter: word[i],
          color: 'orange',
        });
      } else {
        row.push({
          letter: word[i],
          color: 'black',
        });
      }
    }

    this.outputs.update((list) => [...list, row]);

    this.answer.set('');
  }

  getValue(event: Event) {
    const value = (event.target as HTMLInputElement).value.toLowerCase();
    this.answer.set(value);
  }
}
