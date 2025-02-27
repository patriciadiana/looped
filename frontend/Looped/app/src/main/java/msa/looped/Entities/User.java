package msa.looped.Entities;

import com.google.gson.annotations.SerializedName;

public class User {

    @SerializedName("first_name")
    private String first_name;
    @SerializedName("location")
    private String location;
    @SerializedName("username")
    private String username;
    @SerializedName("large_photo_url")
    private String large_photo_url;
    @SerializedName("about_me")
    private String about_me;

    @SerializedName("fave_colors")
    private String fave_colors;

    public String getFave_colors() {
        return fave_colors;
    }

    public void setFave_colors(String fave_colors) {
        this.fave_colors = fave_colors;
    }

    public String getFirst_name() {
        return first_name;
    }

    public void setFirst_name(String first_name) {
        this.first_name = first_name;
    }

    public String getLocation() {
        return location;
    }

    public void setLocation(String location) {
        this.location = location;
    }

    public String getUsername() {
        return username;
    }

    public void setUsername(String username) {
        this.username = username;
    }

    public String getLarge_photo_url() {
        return large_photo_url;
    }

    public void setLarge_photo_url(String large_photo_url) {
        this.large_photo_url = large_photo_url;
    }

    public String getAbout_me() {
        return about_me;
    }

    public void setAbout_me(String about_me) {
        this.about_me = about_me;
    }

    @Override
    public String toString() {
        return "User{" +
                "first_name='" + first_name + '\'' +
                ", location='" + location + '\'' +
                ", username='" + username + '\'' +
                ", large_photo_url='" + large_photo_url + '\'' +
                ", about_me='" + about_me + '\'' +
                ", fave_colors='" + fave_colors + '\'' +
                '}';
    }
}
