package msa.looped.Pages;

import android.app.Activity;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;

import java.util.List;

import msa.looped.Data;
import msa.looped.Entities.Friend;
import msa.looped.Entities.FriendsAdapter;
import msa.looped.R;
import msa.looped.databinding.MyfriendsPageBinding;
import okhttp3.OkHttpClient;

public class MyFriendsPage extends Fragment {

    private MyfriendsPageBinding binding;

    public OkHttpClient client;

    private String apiUrl = Data.getInstance().getApiUrl();

    @Override
    public View onCreateView(
            @NonNull LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {

        binding = MyfriendsPageBinding.inflate(inflater, container, false);
        client = new OkHttpClient();

        List<Friend> friendsList = Data.getFriendslist().getFriends();

        FriendsAdapter adapter = new FriendsAdapter(getContext(), friendsList);
        binding.listFriends.setAdapter(adapter);

        binding.listFriends.setOnItemClickListener((parent, view, position, id) -> {
            Friend selectedFriend = friendsList.get(position);

            FriendProfilePage friendProfileFragment = new FriendProfilePage();

            Bundle bundle = new Bundle();
            bundle.putSerializable("friend", selectedFriend);
            friendProfileFragment.setArguments(bundle);

            loadFragment(friendProfileFragment);
        });

        return binding.getRoot();

    }
    private void loadFragment(Fragment fragment)
    {
        FragmentManager fragmentManager = getActivity().getSupportFragmentManager();
        FragmentTransaction fragmentTransaction = fragmentManager.beginTransaction();
        fragmentTransaction.add(R.id.myfriends_page, fragment);
        fragmentTransaction.commit();
    }

    @Override
    public void onResume() {
        super.onResume();
        FriendsAdapter adapter = new FriendsAdapter(getContext(), Data.getFriendslist().getFriends());
        binding.listFriends.setAdapter(adapter);
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        binding = null;
    }
}
