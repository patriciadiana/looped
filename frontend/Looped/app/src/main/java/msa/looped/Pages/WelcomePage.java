package msa.looped.Pages;

import android.os.Bundle;
import android.provider.Settings;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.navigation.fragment.NavHostFragment;

import android.content.Context;
import android.content.SharedPreferences;
import android.net.Uri;
import android.util.Log;

import com.google.gson.Gson;

import java.io.BufferedReader;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;

import msa.looped.Data;
import msa.looped.Entities.FriendsActivity;
import msa.looped.Entities.Friendslist;
import msa.looped.Entities.ProjectsList;
import msa.looped.Entities.QueuedProjects;
import msa.looped.Entities.UserResponse;
import msa.looped.R;
import msa.looped.databinding.WelcomePageBinding;
import okhttp3.Call;
import okhttp3.Callback;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;

public class WelcomePage extends Fragment {

    private WelcomePageBinding binding;
    private String state;
    public OkHttpClient client;
    private String authorizationCode;
    private String apiUrl = Data.getInstance().getApiUrl();
    private String redirectUri="";
    private String clientId = "4e2b721afe4a3e4a5853efdf287b86cc";

    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        client = new OkHttpClient();
        binding = WelcomePageBinding.inflate(inflater, container, false);
        return binding.getRoot();

    }

    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        binding.button.setOnClickListener(v ->
                NavHostFragment.findNavController(WelcomePage.this)
                        .navigate(R.id.action_HomePage_to_LoginPage)
        );

        if (getArguments() != null) {
            redirectUri = getArguments().getString("redirectUri");
        }
//         requireContext().getFileStreamPath("user_code.txt").delete();
        // we have logged in before -> fetch user id and call backend
        if (requireContext().getFileStreamPath("user_code.txt").exists())
        {
            System.out.println("not our first time logging in!");
            StringBuilder code = new StringBuilder();
            try (FileInputStream fis = requireContext().openFileInput("user_code.txt");
                 BufferedReader reader = new BufferedReader(new InputStreamReader(fis))) {
                String line;
                while ((line = reader.readLine()) != null) {
                    code.append(line);
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
            String result = code.toString();
            getAccessTokenFromBackend(result, "regular");
            NavHostFragment.findNavController(WelcomePage.this)
                    .navigate(R.id.action_WelcomePage_to_homePage);
        }
        else
        {
            System.out.println("this is our first time logging in!");
        }
    }

    @Override
    public void onResume() {
        super.onResume();

        SharedPreferences preferences = requireContext().getSharedPreferences("app_prefs", Context.MODE_PRIVATE);
        state = preferences.getString("state", null);

        Uri data = requireActivity().getIntent().getData();
        if (data != null && data.toString().startsWith("looped://callback")) {
            String authorizationCode = data.getQueryParameter("code");
            String returnedState = data.getQueryParameter("state");

            System.out.println(data);
            if (authorizationCode != null && state.equals(returnedState)) {
                getAccessTokenFromBackend(authorizationCode, "first");
                NavHostFragment.findNavController(WelcomePage.this)
                        .navigate(R.id.action_WelcomePage_to_homePage);
            } else {
                Log.e("ProcessingFragment", "Authorization code or state mismatch.");
            }
        }
    }

    private void getAccessTokenFromBackend(String authorizationCode, String loginType) {

        String backendUrl = apiUrl + "/oauth/"+ loginType + "Login?code=" + authorizationCode;
        System.out.println("backend url: " + backendUrl);

        OkHttpClient client = new OkHttpClient();
        Request request = new Request.Builder()
                .url(backendUrl)
                .post(RequestBody.create(null, new byte[0]))
                .addHeader("Content-Type", "application/json")
                .build();

        client.newCall(request).enqueue(new Callback() {
            @Override
            public void onFailure(Call call, IOException e) {
                e.printStackTrace();
            }

            @Override
            public void onResponse(Call call, Response response) throws IOException {
                if (response.isSuccessful()) {
                    String responseBody = response.body().string();
                    Log.d("BackendResponse", "Token Response: " + responseBody);

                    saveCodeToFile(authorizationCode);
                    fetchCurrentUserProfile(loginType);
                } else {
                    Log.e("BackendResponse", "Error: " + response.code());
                }
            }
        });
    }

    public void saveCodeToFile(String code) {
        try (FileOutputStream fos = requireContext().openFileOutput("user_code.txt", Context.MODE_PRIVATE)) {
            Data.setUserCode(code);
            fos.write(code.getBytes());
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private void fetchCurrentUserProjects() {

        String url = apiUrl + "/main/user/" + Data.getCurrentUser().getUsername() +  "/projects";

        Request request = new Request.Builder()
                .url(url)
                .build();

        client.newCall(request).enqueue(new Callback() {
            @Override
            public void onResponse(@NonNull Call call, @NonNull Response response) throws IOException {
                if (response.isSuccessful()) {
                    final String responseData = response.body().string();

                    Gson gson = new Gson();
                    ProjectsList projectList = gson.fromJson(responseData, ProjectsList.class);
                    Data.setProjectsList(projectList);
                    fetchCurrentUserActivity();

                }
            }

            @Override
            public void onFailure(@NonNull Call call, @NonNull IOException e) {
                e.printStackTrace();
                String errorMessage = e.getMessage();
                System.out.println(errorMessage);
            }
        });
    }
    private void fetchCurrentUserActivity() {

        String url = apiUrl + "/main/user/" + Data.getCurrentUser().getUsername() +  "/friends_activity";

        Request request = new Request.Builder()
                .url(url)
                .build();

        client.newCall(request).enqueue(new Callback() {
            @Override
            public void onResponse(@NonNull Call call, @NonNull Response response) throws IOException {
                if (response.isSuccessful()) {
                    final String responseData = response.body().string();

                    Gson gson = new Gson();
                    FriendsActivity friendsActivity = gson.fromJson(responseData, FriendsActivity.class);
                    Data.setFriendsActivity(friendsActivity);
                    System.out.println(friendsActivity);
                    fetchCurrentUserQueue();

                }
            }

            @Override
            public void onFailure(@NonNull Call call, @NonNull IOException e) {
                e.printStackTrace();
                String errorMessage = e.getMessage();
                System.out.println(errorMessage);
            }
        });
    }
    private void fetchCurrentUserProfile(String loginType) {

        String url = apiUrl + "/main/current_user";
        String userCode = Data.getUserCode();

        if(loginType.equals("first"))
            url = url + "_api/" + userCode;
        else
            url = url + "_db/" + userCode;

        Request request = new Request.Builder()
                .url(url)
                .build();

        client.newCall(request).enqueue(new Callback() {
            @Override
            public void onResponse(@NonNull Call call, @NonNull Response response) throws IOException {
                if (response.isSuccessful()) {
                    final String responseData = response.body().string();

                    Gson gson = new Gson();
                    UserResponse userResponse  = gson.fromJson(responseData, UserResponse.class);
                    Data.setCurrentUser(userResponse.getUser());
                    System.out.println(Data.getCurrentUser());
                    fetchCurrentUserProjects();
                }
            }

            @Override
            public void onFailure(@NonNull Call call, @NonNull IOException e) {
                e.printStackTrace();
                String errorMessage = e.getMessage();
//                getActivity().runOnUiThread(() -> binding.responseTextView.setText("Failed to connect: " + errorMessage));
            }
        });
    }

    private void fetchCurrentUserQueue() {

        String url = apiUrl + "/main/user/" + Data.getCurrentUser().getUsername() +  "/queue";

        Request request = new Request.Builder()
                .url(url)
                .build();

        client.newCall(request).enqueue(new Callback() {
            @Override
            public void onResponse(@NonNull Call call, @NonNull Response response) throws IOException {
                if (response.isSuccessful()) {
                    final String responseData = response.body().string();

                    Gson gson = new Gson();
                    QueuedProjects queuedProjects = gson.fromJson(responseData, QueuedProjects.class);
                    Data.setQueuedProjects(queuedProjects);
                    System.out.println(Data.getQueuedProjects());
                    fetchCurrentUserFriends();

                }
            }

            @Override
            public void onFailure(@NonNull Call call, @NonNull IOException e) {
                e.printStackTrace();
                String errorMessage = e.getMessage();
                System.out.println(errorMessage);
            }
        });
    }

    private void fetchCurrentUserFriends() {

        String url = apiUrl + "/main/user/" + Data.getCurrentUser().getUsername() +  "/friendslist";

        Request request = new Request.Builder()
                .url(url)
                .build();

        client.newCall(request).enqueue(new Callback() {
            @Override
            public void onResponse(@NonNull Call call, @NonNull Response response) throws IOException {
                if (response.isSuccessful()) {
                    final String responseData = response.body().string();

                    Gson gson = new Gson();
                    Friendslist friendslist = gson.fromJson(responseData, Friendslist.class);
                    Data.setFriendslist(friendslist);
                    System.out.println(Data.getFriendslist());

                }
            }

            @Override
            public void onFailure(@NonNull Call call, @NonNull IOException e) {
                e.printStackTrace();
                String errorMessage = e.getMessage();
                System.out.println(errorMessage);
            }
        });
    }


    @Override
    public void onDestroyView() {
        super.onDestroyView();
        binding = null;
    }
}