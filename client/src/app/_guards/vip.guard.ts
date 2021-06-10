import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AccountService } from '../_services/account.service';

@Injectable({
  providedIn: 'root'
})
export class VipGuard implements CanActivate {
  constructor(private accountService: AccountService,private toastr: ToastrService){}
  canActivate():Observable<any> {
    
    return this.accountService.currentUser$.pipe(
      map(user => {
        if(user.roles.includes('VIP')){
          return true;
        }
        this.toastr.error('You cannot enter this area');
        return false;
      })
    );
  }
  
}
