import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-product-carousel',
  imports: [],
  templateUrl: './product-carousel.html',
  styleUrl: './product-carousel.css',
})
export class ProductCarousel implements OnInit {
  images = [
    'https://picsum.photos/1200/400?1',
    'https://picsum.photos/1200/400?2',
    'https://picsum.photos/1200/400?3',
  ];

  currentIndex = 0;

  ngOnInit() {
    setInterval(() => {
      this.currentIndex++;

      if (this.currentIndex >= this.images.length) {
        this.currentIndex = 0;
      }
    }, 3000);
  }
}
