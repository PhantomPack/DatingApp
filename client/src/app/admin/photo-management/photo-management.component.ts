import { Component, Input, OnInit } from '@angular/core';
import { Photo } from 'src/app/_models/Photo';
import { AdminService } from 'src/app/_services/admin.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  @Input()
  photos: Photo[] = [];
  constructor(private adminService: AdminService) { }

  ngOnInit(): void {
    this.getPhotosForApproval();
  }

  getPhotosForApproval(){
    this.adminService.getPhotosForApproval().subscribe(photos =>{
      this.photos = photos as Photo[];
    });
  }

  approvePhoto(id: number){
    this.adminService.approvePhoto(id).subscribe(()=>{
        this.getPhotosForApproval();      
    });
  }
  
  rejectPhoto(id: number){
    this.adminService.rejectPhoto(id).subscribe(()=>{
      this.getPhotosForApproval();  
    });
  }
  



}
