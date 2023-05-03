import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../services/account.service';
import { Observable, of } from 'rxjs';
import { User } from '../models/user';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {}
  currentUser$: Observable<User | null> = of(null);

  constructor(
    public accountService: AccountService, 
    private router: Router, 
    private toastr: ToastrService) { }

  ngOnInit(): void {
    this.currentUser$ = this.accountService.currentUser$;
  }

  login() {
    this.accountService.login(this.model)
      .subscribe({
        next: () => this.router.navigateByUrl('/members'),
        error: error => this.toastr.error(error.error)
      });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}
