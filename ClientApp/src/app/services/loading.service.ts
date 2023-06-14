import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  busyRequestCount = 0;

  constructor(private spinnerService: NgxSpinnerService) { }

  show() {
    this.busyRequestCount++;
    this.spinnerService.show(undefined, {
      type: 'line-scale-party',
      bdColor: 'rgba(0, 0, 0, 0.8)',
      color: '#fff',
      size: 'medium',
    });
  }

  hide() {
    this.busyRequestCount--;
    
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide();
    }
  }
}
