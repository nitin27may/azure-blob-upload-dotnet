import { Component } from '@angular/core';
import { HttpClient, HttpEventType, HttpRequest } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  loader: boolean;
  imageDetails: any;
  formData: FormData;

  files: any;
  constructor(private httpClient: HttpClient) {
    this.loader = false;
    this.getAllFiles();
  }

  // uploadFile(files: any): void {
  //   if (files.length === 0) {
  //     return;
  //   }
  //   let fileToUpload = <File>files[0];
  //   this.formData = new FormData();
  //  // for (const file of files) {
  //     this.formData.append('asset', fileToUpload);
  //   //}
  //   this.uploadtoServer(this.formData);
  // }

  // uploadtoServer(fromData: any): void {
  //   this.loader = true;
  //   this.httpClient.post('/api/File', fromData).subscribe(
  //     (data: any) => {
  //       this.imageDetails = data;
  //       this.loader = false;
  //       this.getAllFiles();
  //     },
  //     (error) => {
  //       this.loader = false;
  //     }
  //   );
  // }


  getAllFiles(){
    this.loader = true;
    this.httpClient.get('/api/File/videos').subscribe(
      (data: any) => {
        data.sort((a: any, b: any) => {
          return new Date(b.createdOn).getTime() - new Date(a.createdOn).getTime(); // descending
        })
        this.files = data;
        this.loader = false;
      },
      (error) => {
        this.loader = false;
      }
    );
  }



  working = false;
  uploadFile: File | null;
  uploadFileLabel: string | undefined = 'Choose an image to upload';
  uploadProgress: number;
  uploadUrl: string;

  handleFileInput(files: FileList) {
    if (files.length > 0) {
      this.uploadFile = files.item(0);
      this.uploadFileLabel = this.uploadFile?.name;
    }
  }

  upload() {
    if (!this.uploadFile) {
      alert('Choose a file to upload first');
      return;
    }

    const formData = new FormData();
    formData.append(this.uploadFile.name, this.uploadFile);

    const url = 'api/File';
    const uploadReq = new HttpRequest('POST', url, formData, {
      reportProgress: true,
    });

    this.uploadUrl = '';
    this.uploadProgress = 0;
    this.working = true;

    this.httpClient.request(uploadReq).subscribe((event: any) => {
      if (event.type === HttpEventType.UploadProgress) {
        this.uploadProgress = Math.round((100 * event.loaded) / event.total);
      } else if (event.type === HttpEventType.Response) {
        this.uploadUrl = event.body.fileURL;
        this.uploaded();
      }
    }, (error: any) => {
      console.error(error);
    }).add(() => {
      this.working = false;
    });
  }

  uploaded(){
    this.getAllFiles();
    this.uploadFileLabel = "";
    setTimeout(() => {
      this.uploadUrl = "";
    }, 2500);
  }
}
